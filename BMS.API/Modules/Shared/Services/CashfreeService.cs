using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BMS.API.Modules.Shared.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BMS.API.Modules.Shared.Services
{
    /// <summary>
    /// Thin REST wrapper around the Cashfree Payment Gateway "Orders API" (v2023-08-01).
    /// No official Cashfree .NET SDK is used - it's a small, well-documented JSON API, so a
    /// plain typed HttpClient keeps the dependency footprint minimal.
    /// Docs: https://docs.cashfree.com/reference/pg-create-order
    /// </summary>
    public class CashfreeService : ICashfreeService
    {
        private readonly HttpClient _http;
        private readonly CashfreeOptions _options;
        private readonly ILogger<CashfreeService> _logger;

        public CashfreeService(HttpClient http, IOptions<CashfreeOptions> options, ILogger<CashfreeService> logger)
        {
            _options = options.Value;
            _logger = logger;

            http.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            http.DefaultRequestHeaders.Add("x-client-id", _options.AppId);
            http.DefaultRequestHeaders.Add("x-client-secret", _options.SecretKey);
            http.DefaultRequestHeaders.Add("x-api-version", _options.ApiVersion);
            _http = http;
        }

        private class CreateOrderRequest
        {
            [JsonPropertyName("order_id")] public string OrderId { get; set; } = string.Empty;
            [JsonPropertyName("order_amount")] public decimal OrderAmount { get; set; }
            [JsonPropertyName("order_currency")] public string OrderCurrency { get; set; } = "INR";
            [JsonPropertyName("customer_details")] public CustomerDetails CustomerDetails { get; set; } = new();
            [JsonPropertyName("order_meta")] public OrderMeta OrderMeta { get; set; } = new();
        }

        private class CustomerDetails
        {
            [JsonPropertyName("customer_id")] public string CustomerId { get; set; } = string.Empty;
            [JsonPropertyName("customer_phone")] public string CustomerPhone { get; set; } = string.Empty;
            [JsonPropertyName("customer_email")] public string? CustomerEmail { get; set; }
        }

        private class OrderMeta
        {
            [JsonPropertyName("return_url")] public string ReturnUrl { get; set; } = string.Empty;
            [JsonPropertyName("notify_url")] public string NotifyUrl { get; set; } = string.Empty;
        }

        private class OrderResponse
        {
            [JsonPropertyName("order_id")] public string? OrderId { get; set; }
            [JsonPropertyName("order_status")] public string? OrderStatus { get; set; }
            [JsonPropertyName("payment_session_id")] public string? PaymentSessionId { get; set; }
            [JsonPropertyName("message")] public string? Message { get; set; }
        }

        public async Task<CashfreeOrderResult> CreateOrderAsync(
            string orderId,
            decimal amount,
            string customerId,
            string customerPhone,
            string? customerEmail,
            string returnUrl,
            string notifyUrl)
        {
            var payload = new CreateOrderRequest
            {
                OrderId = orderId,
                OrderAmount = amount,
                OrderCurrency = "INR",
                CustomerDetails = new CustomerDetails
                {
                    CustomerId = customerId,
                    // Cashfree requires a non-empty phone; fall back to a placeholder rather
                    // than send an empty string, which Cashfree rejects outright.
                    CustomerPhone = string.IsNullOrWhiteSpace(customerPhone) ? "9999999999" : customerPhone,
                    CustomerEmail = string.IsNullOrWhiteSpace(customerEmail) ? null : customerEmail,
                },
                OrderMeta = new OrderMeta { ReturnUrl = returnUrl, NotifyUrl = notifyUrl },
            };

            try
            {
                using var response = await _http.PostAsJsonAsync("orders", payload);
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Cashfree create-order failed ({Status}): {Body}", response.StatusCode, raw);
                    var errBody = TryDeserialize(raw);
                    return new CashfreeOrderResult
                    {
                        Success = false,
                        ErrorMessage = errBody?.Message ?? $"Cashfree order creation failed ({(int)response.StatusCode})",
                    };
                }

                var body = TryDeserialize(raw);
                if (body?.PaymentSessionId == null)
                {
                    return new CashfreeOrderResult { Success = false, ErrorMessage = "Cashfree did not return a payment session." };
                }

                return new CashfreeOrderResult
                {
                    Success = true,
                    OrderId = body.OrderId,
                    PaymentSessionId = body.PaymentSessionId,
                    OrderStatus = body.OrderStatus,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cashfree create-order request threw");
                return new CashfreeOrderResult { Success = false, ErrorMessage = "Could not reach the payment gateway. Please try again." };
            }
        }

        public async Task<CashfreeOrderStatusResult> GetOrderStatusAsync(string orderId)
        {
            try
            {
                using var response = await _http.GetAsync($"orders/{Uri.EscapeDataString(orderId)}");
                var raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new CashfreeOrderStatusResult
                    {
                        Success = false,
                        ErrorMessage = $"Order lookup failed ({(int)response.StatusCode})",
                    };
                }

                var body = TryDeserialize(raw);
                return new CashfreeOrderStatusResult { Success = true, OrderStatus = body?.OrderStatus };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cashfree get-order-status request threw");
                return new CashfreeOrderStatusResult { Success = false, ErrorMessage = "Could not reach the payment gateway." };
            }
        }

        private static OrderResponse? TryDeserialize(string raw)
        {
            try
            {
                return JsonSerializer.Deserialize<OrderResponse>(raw);
            }
            catch
            {
                return null;
            }
        }

        public bool VerifyWebhookSignature(string timestamp, string rawBody, string signature)
        {
            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature)) return false;

            var signedPayload = timestamp + rawBody;
            var keyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
            var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

            using var hmac = new HMACSHA256(keyBytes);
            var computedHash = hmac.ComputeHash(payloadBytes);
            var computedSignature = Convert.ToBase64String(computedHash);

            var computedBytes = Encoding.UTF8.GetBytes(computedSignature);
            var providedBytes = Encoding.UTF8.GetBytes(signature);
            return computedBytes.Length == providedBytes.Length && CryptographicOperations.FixedTimeEquals(computedBytes, providedBytes);
        }
    }
}
