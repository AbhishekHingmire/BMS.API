import uuid

cities = {
    'Pune': ['Kothrud', 'Viman Nagar', 'Hinjewadi', 'Kharadi', 'Wakad', 'Baner', 'Aundh', 'Magarpatta', 'Hadapsar', 'Koregaon Park', 'Kalyani Nagar', 'Shivaji Nagar', 'Swargate', 'Pimpri', 'Chinchwad', 'Camp', 'Deccan Gymkhana', 'Kondhwa', 'Katraj', 'Vishrantwadi', 'Bavdhan', 'Pashan', 'Balewadi', 'Erandwane', 'Fatima Nagar', 'Nigdi', 'Pimple Saudagar', 'Pimple Nilakh', 'Pimple Gurav', 'Thergaon', 'Ravet', 'Tathawade', 'Narhe', 'Dhayari', 'Sinhagad Road', 'Karve Nagar', 'Warje'],
    'Mumbai': ['Andheri', 'Bandra', 'Juhu', 'Colaba', 'Malabar Hill', 'Powai', 'Borivali', 'Goregaon', 'Malad', 'Kandivali', 'Dadar', 'Worli', 'Lower Parel', 'South Mumbai', 'Navi Mumbai', 'Vashi', 'Thane', 'Chembur', 'Ghatkopar', 'Kurla', 'Vile Parle', 'Santacruz', 'Khar', 'Sion', 'Matunga', 'Mahim', 'Prabhadevi', 'Byculla', 'Marine Drive', 'Nariman Point', 'Fort', 'Churchgate', 'Cuffe Parade', 'Mulund', 'Bhandup', 'Vikhroli', 'Kanjurmarg'],
    'Hyderabad': ['Banjara Hills', 'Jubilee Hills', 'HITEC City', 'Madhapur', 'Gachibowli', 'Kondapur', 'Kukatpally', 'Miyapur', 'Ameerpet', 'SR Nagar', 'Begumpet', 'Secunderabad', 'Tarnaka', 'Uppal', 'Dilsukhnagar', 'LB Nagar', 'Charminar', 'Mehdipatnam', 'Tolichowki', 'Manikonda', 'Nanakramguda', 'Somajiguda', 'Panjagutta', 'Khairatabad', 'Abids', 'Koti', 'Himayatnagar', 'Kachiguda', 'Nampally'],
    'Bangalore': ['Koramangala', 'Indiranagar', 'Jayanagar', 'JP Nagar', 'BTM Layout', 'HSR Layout', 'Whitefield', 'Electronic City', 'Marathahalli', 'Bellandur', 'Sarjapur Road', 'Outer Ring Road', 'Hebbal', 'Yelahanka', 'Malleshwaram', 'Rajajinagar', 'Basavanagudi', 'Frazer Town', 'Cox Town', 'Cooke Town', 'Kammanahalli', 'Banaswadi', 'KR Puram', 'CV Raman Nagar', 'Mahadevapura', 'Brookefield', 'Kundalahalli', 'Doddanekundi', 'RT Nagar', 'Sanjaynagar'],
    'Solapur': ['Navi Peth', 'Sadar Bazar', 'Hotgi Road', 'Jule Solapur', 'Ashok Chowk', 'Saat Rasta', 'MIDC', 'Majrewadi', 'Dufferin Chowk', 'Balives', 'Bhavani Peth', 'Gold Finch Peth', 'Sakhar Peth', 'South Kasba', 'North Kasba', 'Soregaon', 'Kumthe', 'Akkalkot Road', 'Vijapur Road', 'Railway Station Area'],
    'Delhi': ['Connaught Place', 'Karol Bagh', 'Paharganj', 'Chandni Chowk', 'Chawri Bazar', 'Daryaganj', 'Pitampura', 'Rohini', 'Shalimar Bagh', 'Model Town', 'Civil Lines', 'Kamla Nagar', 'Mukherjee Nagar', 'GTB Nagar', 'Rajouri Garden', 'Punjabi Bagh', 'Paschim Vihar', 'Janakpuri', 'Dwarka', 'Vasant Kunj', 'Vasant Vihar', 'Hauz Khas', 'Green Park', 'South Extension', 'Defence Colony', 'Lajpat Nagar', 'Greater Kailash', 'Saket', 'Malviya Nagar', 'Chhatarpur', 'Laxmi Nagar', 'Preet Vihar', 'Mayur Vihar', 'Patparganj', 'Shahdara', 'Anand Vihar']
}

with open('seed_cities.sql', 'w') as f:
    for city, areas in cities.items():
        var_name = '@' + city.replace(' ', '') + 'Id'
        f.write(f'DECLARE {var_name} UNIQUEIDENTIFIER = NEWID();\n')
        f.write(f\"INSERT INTO Cities (Id, Name) VALUES ({var_name}, '{city}');\n\")
        for area in areas:
            clean_area = area.replace(\"'\", \"''\")
            f.write(f\"INSERT INTO Localities (Id, CityId, Name) VALUES (NEWID(), {var_name}, '{clean_area}');\n\")
        f.write('\n')

print(\"Generated seed_cities.sql\")
