-- Insert 50 test company records for LinhGo.ERP
-- Generated on 2025-12-10

-- Clear existing test data (optional)
-- DELETE FROM companies WHERE code LIKE 'TEST%';

INSERT INTO companies (
    id, name, code, tax_id, registration_number, email, phone, website,
    address_line1, address_line2, city, state, postal_code, country, industry,
    established_date, is_active, subscription_start_date, subscription_end_date,
    subscription_plan, currency, time_zone, language, logo, created_at, updated_at,
    created_by, updated_by, deleted_by, deleted_at, is_deleted
) VALUES
-- Technology Companies (USA)
('550e8400-e29b-41d4-a716-446655440001', 'TechVision Solutions Inc', 'TECH001', '12-3456789', 'REG-US-001', 'contact@techvision.com', '+1-555-0101', 'https://www.techvision.com',
 '123 Silicon Valley Blvd', 'Suite 100', 'San Francisco', 'CA', '94102', 'USA', 'Technology',
 '2018-03-15 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'USD', 'America/Los_Angeles', 'en-US', 'https://logo.techvision.com/logo.png', NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

('550e8400-e29b-41d4-a716-446655440002', 'CloudMaster Technologies', 'CLOUD001', '23-4567890', 'REG-US-002', 'hello@cloudmaster.io', '+1-555-0102', 'https://www.cloudmaster.io',
 '456 Tech Park Drive', 'Building A', 'Austin', 'TX', '78701', 'USA', 'Technology',
 '2019-06-20 00:00:00+00', true, '2024-02-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'USD', 'America/Chicago', 'en-US', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

('550e8400-e29b-41d4-a716-446655440003', 'DataSync Corporation', 'DATA001', '34-5678901', 'REG-US-003', 'info@datasync.com', '+1-555-0103', 'https://www.datasync.com',
 '789 Innovation Way', NULL, 'Seattle', 'WA', '98101', 'USA', 'Technology',
 '2017-09-10 00:00:00+00', true, '2024-03-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'USD', 'America/Los_Angeles', 'en-US', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Technology Companies (Vietnam)
('550e8400-e29b-41d4-a716-446655440004', 'Vietnam Software Solutions', 'VN001', '0123456789', 'REG-VN-001', 'contact@vnsoftware.vn', '+84-24-3456-7890', 'https://www.vnsoftware.vn',
 '123 Hai Ba Trung', 'Floor 5', 'Hanoi', 'Hanoi', '100000', 'Vietnam', 'Technology',
 '2020-01-15 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'VND', 'Asia/Ho_Chi_Minh', 'vi-VN', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

('550e8400-e29b-41d4-a716-446655440005', 'Saigon Tech Hub', 'SAIGON001', '0234567890', 'REG-VN-002', 'hello@saigontech.vn', '+84-28-3456-7891', 'https://www.saigontech.vn',
 '456 Nguyen Hue', 'District 1', 'Ho Chi Minh City', 'HCMC', '700000', 'Vietnam', 'Technology',
 '2019-05-20 00:00:00+00', true, '2024-02-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'VND', 'Asia/Ho_Chi_Minh', 'vi-VN', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Manufacturing Companies
('550e8400-e29b-41d4-a716-446655440006', 'Global Manufacturing Ltd', 'MFG001', '45-6789012', 'REG-US-004', 'sales@globalmanufacturing.com', '+1-555-0104', 'https://www.globalmanufacturing.com',
 '321 Industrial Blvd', 'Zone B', 'Detroit', 'MI', '48201', 'USA', 'Manufacturing',
 '2015-11-30 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'USD', 'America/Detroit', 'en-US', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

('550e8400-e29b-41d4-a716-446655440007', 'Precision Parts Inc', 'PREC001', '56-7890123', 'REG-US-005', 'contact@precisionparts.com', '+1-555-0105', 'https://www.precisionparts.com',
 '654 Factory Road', NULL, 'Cleveland', 'OH', '44101', 'USA', 'Manufacturing',
 '2016-08-22 00:00:00+00', true, '2024-03-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'USD', 'America/New_York', 'en-US', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Retail Companies
('550e8400-e29b-41d4-a716-446655440008', 'Urban Retail Group', 'RETAIL001', '67-8901234', 'REG-UK-001', 'info@urbanretail.co.uk', '+44-20-7123-4567', 'https://www.urbanretail.co.uk',
 '10 Oxford Street', NULL, 'London', 'England', 'W1D 1BS', 'United Kingdom', 'Retail',
 '2018-04-12 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'GBP', 'Europe/London', 'en-GB', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

('550e8400-e29b-41d4-a716-446655440009', 'Fashion First Store', 'FASHION001', '78-9012345', 'REG-FR-001', 'contact@fashionfirst.fr', '+33-1-23-45-67-89', 'https://www.fashionfirst.fr',
 '25 Champs-Élysées', NULL, 'Paris', 'Île-de-France', '75008', 'France', 'Retail',
 '2017-02-28 00:00:00+00', true, '2024-02-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'EUR', 'Europe/Paris', 'fr-FR', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Healthcare Companies
('550e8400-e29b-41d4-a716-446655440010', 'HealthCare Plus', 'HEALTH001', '89-0123456', 'REG-US-006', 'contact@healthcareplus.com', '+1-555-0106', 'https://www.healthcareplus.com',
 '100 Medical Center Drive', 'Building C', 'Boston', 'MA', '02101', 'USA', 'Healthcare',
 '2016-07-15 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'USD', 'America/New_York', 'en-US', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Financial Services
('550e8400-e29b-41d4-a716-446655440011', 'FinTech Innovations', 'FIN001', '90-1234567', 'REG-SG-001', 'info@fintechinnovations.sg', '+65-6123-4567', 'https://www.fintechinnovations.sg',
 '1 Marina Boulevard', '#15-01', 'Singapore', NULL, '018989', 'Singapore', 'Finance',
 '2019-03-10 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'SGD', 'Asia/Singapore', 'en-SG', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

('550e8400-e29b-41d4-a716-446655440012', 'Digital Banking Solutions', 'BANK001', '01-2345678', 'REG-HK-001', 'hello@digitalbanking.hk', '+852-2123-4567', 'https://www.digitalbanking.hk',
 '88 Queensway', 'Tower 2, 20F', 'Hong Kong', NULL, '00000', 'Hong Kong', 'Finance',
 '2018-11-22 00:00:00+00', true, '2024-02-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'HKD', 'Asia/Hong_Kong', 'en-HK', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- E-commerce
('550e8400-e29b-41d4-a716-446655440013', 'ShopOnline Global', 'ECOM001', '11-2345679', 'REG-US-007', 'support@shoponline.com', '+1-555-0107', 'https://www.shoponline.com',
 '500 Commerce Street', NULL, 'New York', 'NY', '10001', 'USA', 'E-commerce',
 '2019-01-05 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'USD', 'America/New_York', 'en-US', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Construction
('550e8400-e29b-41d4-a716-446655440014', 'BuildRight Construction', 'CONST001', '22-3456780', 'REG-AU-001', 'info@buildright.com.au', '+61-2-9123-4567', 'https://www.buildright.com.au',
 '123 Construction Way', NULL, 'Sydney', 'NSW', '2000', 'Australia', 'Construction',
 '2017-06-18 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'AUD', 'Australia/Sydney', 'en-AU', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Logistics
('550e8400-e29b-41d4-a716-446655440015', 'FastShip Logistics', 'LOG001', '33-4567891', 'REG-DE-001', 'contact@fastship.de', '+49-30-1234-5678', 'https://www.fastship.de',
 '45 Logistikstraße', NULL, 'Berlin', 'Berlin', '10115', 'Germany', 'Logistics',
 '2018-09-25 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'EUR', 'Europe/Berlin', 'de-DE', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Education
('550e8400-e29b-41d4-a716-446655440016', 'EduTech Academy', 'EDU001', '44-5678902', 'REG-CA-001', 'info@edutech.ca', '+1-416-123-4567', 'https://www.edutech.ca',
 '200 University Avenue', 'Suite 300', 'Toronto', 'ON', 'M5H 3C6', 'Canada', 'Education',
 '2019-08-30 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'CAD', 'America/Toronto', 'en-CA', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Telecommunications
('550e8400-e29b-41d4-a716-446655440017', 'TeleConnect Services', 'TELE001', '55-6789013', 'REG-JP-001', 'contact@teleconnect.jp', '+81-3-1234-5678', 'https://www.teleconnect.jp',
 '1-1-1 Shibuya', 'Shibuya-ku', 'Tokyo', 'Tokyo', '150-0002', 'Japan', 'Telecommunications',
 '2016-12-12 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'JPY', 'Asia/Tokyo', 'ja-JP', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Hospitality
('550e8400-e29b-41d4-a716-446655440018', 'Grand Hotels International', 'HOTEL001', '66-7890124', 'REG-AE-001', 'reservations@grandhotels.ae', '+971-4-123-4567', 'https://www.grandhotels.ae',
 'Sheikh Zayed Road', 'Downtown Dubai', 'Dubai', 'Dubai', '00000', 'United Arab Emirates', 'Hospitality',
 '2017-03-20 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'AED', 'Asia/Dubai', 'ar-AE', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Media & Entertainment
('550e8400-e29b-41d4-a716-446655440019', 'StreamMax Media', 'MEDIA001', '77-8901235', 'REG-US-008', 'contact@streammax.com', '+1-555-0108', 'https://www.streammax.com',
 '1000 Entertainment Plaza', NULL, 'Los Angeles', 'CA', '90028', 'USA', 'Media',
 '2019-05-15 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'USD', 'America/Los_Angeles', 'en-US', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Automotive
('550e8400-e29b-41d4-a716-446655440020', 'AutoTech Motors', 'AUTO001', '88-9012346', 'REG-KR-001', 'info@autotechmotors.kr', '+82-2-1234-5678', 'https://www.autotechmotors.kr',
 '123 Gangnam-daero', 'Gangnam-gu', 'Seoul', 'Seoul', '06000', 'South Korea', 'Automotive',
 '2018-07-08 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'KRW', 'Asia/Seoul', 'ko-KR', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- More Technology Companies (Various Countries)
('550e8400-e29b-41d4-a716-446655440021', 'CodeCraft Studios', 'CODE001', '99-0123457', 'REG-PL-001', 'hello@codecraft.pl', '+48-22-123-4567', 'https://www.codecraft.pl',
 'ul. Marszałkowska 1', NULL, 'Warsaw', 'Mazowieckie', '00-624', 'Poland', 'Technology',
 '2020-02-14 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Starter', 'PLN', 'Europe/Warsaw', 'pl-PL', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

('550e8400-e29b-41d4-a716-446655440022', 'DevOps Masters', 'DEVOPS001', '00-1234568', 'REG-NL-001', 'info@devopsmasters.nl', '+31-20-123-4567', 'https://www.devopsmasters.nl',
 'Herengracht 450', NULL, 'Amsterdam', 'North Holland', '1017 CA', 'Netherlands', 'Technology',
 '2019-11-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'EUR', 'Europe/Amsterdam', 'nl-NL', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Pharmaceutical
('550e8400-e29b-41d4-a716-446655440023', 'MediPharm Solutions', 'PHARMA001', '10-2345679', 'REG-CH-001', 'contact@medipharm.ch', '+41-44-123-4567', 'https://www.medipharm.ch',
 'Bahnhofstrasse 100', NULL, 'Zurich', 'Zurich', '8001', 'Switzerland', 'Pharmaceutical',
 '2016-05-20 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'CHF', 'Europe/Zurich', 'de-CH', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Energy
('550e8400-e29b-41d4-a716-446655440024', 'GreenEnergy Corp', 'ENERGY001', '20-3456780', 'REG-NO-001', 'info@greenenergy.no', '+47-22-12-34-56', 'https://www.greenenergy.no',
 'Karl Johans gate 1', NULL, 'Oslo', 'Oslo', '0154', 'Norway', 'Energy',
 '2017-09-15 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'NOK', 'Europe/Oslo', 'no-NO', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Agriculture
('550e8400-e29b-41d4-a716-446655440025', 'AgroTech Innovations', 'AGRO001', '30-4567891', 'REG-BR-001', 'contato@agrotech.com.br', '+55-11-1234-5678', 'https://www.agrotech.com.br',
 'Avenida Paulista 1000', 'Conj 101', 'São Paulo', 'SP', '01310-100', 'Brazil', 'Agriculture',
 '2018-12-05 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'BRL', 'America/Sao_Paulo', 'pt-BR', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Insurance
('550e8400-e29b-41d4-a716-446655440026', 'SecureLife Insurance', 'INS001', '40-5678902', 'REG-IT-001', 'info@securelife.it', '+39-02-1234-5678', 'https://www.securelife.it',
 'Via Montenapoleone 1', NULL, 'Milan', 'Lombardy', '20121', 'Italy', 'Insurance',
 '2017-04-18 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'EUR', 'Europe/Rome', 'it-IT', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Real Estate
('550e8400-e29b-41d4-a716-446655440027', 'Prime Properties Group', 'REAL001', '50-6789013', 'REG-ES-001', 'info@primeproperties.es', '+34-91-123-4567', 'https://www.primeproperties.es',
 'Gran Vía 1', NULL, 'Madrid', 'Madrid', '28013', 'Spain', 'Real Estate',
 '2018-06-22 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'EUR', 'Europe/Madrid', 'es-ES', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Consulting
('550e8400-e29b-41d4-a716-446655440028', 'Business Advisors Ltd', 'CONSULT001', '60-7890124', 'REG-SE-001', 'contact@businessadvisors.se', '+46-8-123-4567', 'https://www.businessadvisors.se',
 'Drottninggatan 1', NULL, 'Stockholm', 'Stockholm', '111 51', 'Sweden', 'Consulting',
 '2019-02-10 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'SEK', 'Europe/Stockholm', 'sv-SE', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Food & Beverage
('550e8400-e29b-41d4-a716-446655440029', 'Global Foods Co', 'FOOD001', '70-8901235', 'REG-TH-001', 'info@globalfoods.th', '+66-2-123-4567', 'https://www.globalfoods.th',
 '123 Sukhumvit Road', 'Floor 10', 'Bangkok', 'Bangkok', '10110', 'Thailand', 'Food & Beverage',
 '2017-08-15 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'THB', 'Asia/Bangkok', 'th-TH', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Sports & Recreation
('550e8400-e29b-41d4-a716-446655440030', 'FitLife Sports', 'SPORT001', '80-9012346', 'REG-NZ-001', 'hello@fitlifesports.co.nz', '+64-9-123-4567', 'https://www.fitlifesports.co.nz',
 '100 Queen Street', NULL, 'Auckland', 'Auckland', '1010', 'New Zealand', 'Sports',
 '2019-03-28 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Starter', 'NZD', 'Pacific/Auckland', 'en-NZ', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Additional Tech Startups
('550e8400-e29b-41d4-a716-446655440031', 'AI Innovations Lab', 'AI001', '91-0123457', 'REG-IL-001', 'info@ailab.co.il', '+972-3-123-4567', 'https://www.ailab.co.il',
 'Rothschild Blvd 1', NULL, 'Tel Aviv', 'Tel Aviv', '66881', 'Israel', 'Technology',
 '2020-04-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Starter', 'ILS', 'Asia/Jerusalem', 'he-IL', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

('550e8400-e29b-41d4-a716-446655440032', 'Blockchain Solutions Inc', 'BLOCK001', '02-1234568', 'REG-US-009', 'contact@blockchainsolutions.com', '+1-555-0109', 'https://www.blockchainsolutions.com',
 '200 Crypto Avenue', NULL, 'Miami', 'FL', '33101', 'USA', 'Technology',
 '2020-06-10 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'USD', 'America/New_York', 'en-US', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- More Manufacturing
('550e8400-e29b-41d4-a716-446655440033', 'Advanced Materials Corp', 'MAT001', '12-2345679', 'REG-CN-001', 'info@advancedmaterials.cn', '+86-21-1234-5678', 'https://www.advancedmaterials.cn',
 'Pudong Avenue 1', 'Building 3', 'Shanghai', 'Shanghai', '200120', 'China', 'Manufacturing',
 '2016-10-20 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'CNY', 'Asia/Shanghai', 'zh-CN', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Chemical Industry
('550e8400-e29b-41d4-a716-446655440034', 'ChemTech Industries', 'CHEM001', '22-3456780', 'REG-IN-001', 'contact@chemtech.in', '+91-22-1234-5678', 'https://www.chemtech.in',
 'Marine Drive 1', NULL, 'Mumbai', 'Maharashtra', '400002', 'India', 'Chemical',
 '2017-12-01 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'INR', 'Asia/Kolkata', 'en-IN', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- More Retail
('550e8400-e29b-41d4-a716-446655440035', 'MegaMart Retail Chain', 'MEGA001', '32-4567891', 'REG-MX-001', 'info@megamart.mx', '+52-55-1234-5678', 'https://www.megamart.mx',
 'Paseo de la Reforma 1', NULL, 'Mexico City', 'CDMX', '06600', 'Mexico', 'Retail',
 '2018-01-15 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'MXN', 'America/Mexico_City', 'es-MX', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Aviation
('550e8400-e29b-41d4-a716-446655440036', 'SkyHigh Airlines', 'SKY001', '42-5678902', 'REG-MY-001', 'reservations@skyhigh.my', '+60-3-1234-5678', 'https://www.skyhigh.my',
 'Jalan Sultan Ismail 1', NULL, 'Kuala Lumpur', 'Wilayah Persekutuan', '50250', 'Malaysia', 'Aviation',
 '2017-05-22 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'MYR', 'Asia/Kuala_Lumpur', 'ms-MY', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Publishing
('550e8400-e29b-41d4-a716-446655440037', 'Digital Publishing House', 'PUB001', '52-6789013', 'REG-AT-001', 'info@digitalpublishing.at', '+43-1-123-4567', 'https://www.digitalpublishing.at',
 'Stephansplatz 1', NULL, 'Vienna', 'Vienna', '1010', 'Austria', 'Publishing',
 '2018-09-10 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'EUR', 'Europe/Vienna', 'de-AT', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Tourism
('550e8400-e29b-41d4-a716-446655440038', 'Adventure Tours Inc', 'TOUR001', '62-7890124', 'REG-ZA-001', 'bookings@adventuretours.co.za', '+27-21-123-4567', 'https://www.adventuretours.co.za',
 'Long Street 1', NULL, 'Cape Town', 'Western Cape', '8001', 'South Africa', 'Tourism',
 '2019-04-05 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'ZAR', 'Africa/Johannesburg', 'en-ZA', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Legal Services
('550e8400-e29b-41d4-a716-446655440039', 'International Law Partners', 'LAW001', '72-8901235', 'REG-BE-001', 'contact@lawpartners.be', '+32-2-123-4567', 'https://www.lawpartners.be',
 'Avenue Louise 1', NULL, 'Brussels', 'Brussels', '1000', 'Belgium', 'Legal',
 '2017-11-18 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'EUR', 'Europe/Brussels', 'fr-BE', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Security Services
('550e8400-e29b-41d4-a716-446655440040', 'CyberSecure Solutions', 'SEC001', '82-9012346', 'REG-FI-001', 'info@cybersecure.fi', '+358-9-1234-5678', 'https://www.cybersecure.fi',
 'Mannerheimintie 1', NULL, 'Helsinki', 'Uusimaa', '00100', 'Finland', 'Security',
 '2019-07-25 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'EUR', 'Europe/Helsinki', 'fi-FI', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- More E-commerce
('550e8400-e29b-41d4-a716-446655440041', 'MarketPlace Online', 'MARKET001', '92-0123457', 'REG-ID-001', 'support@marketplace.id', '+62-21-1234-5678', 'https://www.marketplace.id',
 'Jalan Sudirman 1', 'Tower A', 'Jakarta', 'Jakarta', '12190', 'Indonesia', 'E-commerce',
 '2018-11-08 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'IDR', 'Asia/Jakarta', 'id-ID', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Entertainment
('550e8400-e29b-41d4-a716-446655440042', 'Gaming Studios Pro', 'GAME001', '03-1234568', 'REG-PH-001', 'hello@gamingstudios.ph', '+63-2-1234-5678', 'https://www.gamingstudios.ph',
 'Ayala Avenue 1', 'Makati', 'Manila', 'Metro Manila', '1200', 'Philippines', 'Entertainment',
 '2020-03-12 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Starter', 'PHP', 'Asia/Manila', 'en-PH', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Environmental Services
('550e8400-e29b-41d4-a716-446655440043', 'EcoSolutions Global', 'ECO001', '13-2345679', 'REG-DK-001', 'info@ecosolutions.dk', '+45-33-12-34-56', 'https://www.ecosolutions.dk',
 'Vesterbrogade 1', NULL, 'Copenhagen', 'Capital Region', '1620', 'Denmark', 'Environmental',
 '2017-02-20 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'DKK', 'Europe/Copenhagen', 'da-DK', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Telecommunications (More)
('550e8400-e29b-41d4-a716-446655440044', 'NetConnect Services', 'NET001', '23-3456780', 'REG-IE-001', 'contact@netconnect.ie', '+353-1-123-4567', 'https://www.netconnect.ie',
 'O''Connell Street 1', NULL, 'Dublin', 'Leinster', 'D01 F5P2', 'Ireland', 'Telecommunications',
 '2018-05-30 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'EUR', 'Europe/Dublin', 'en-IE', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Architecture
('550e8400-e29b-41d4-a716-446655440045', 'UrbanDesign Architects', 'ARCH001', '33-4567891', 'REG-PT-001', 'info@urbandesign.pt', '+351-21-123-4567', 'https://www.urbandesign.pt',
 'Avenida da Liberdade 1', NULL, 'Lisbon', 'Lisbon', '1250-096', 'Portugal', 'Architecture',
 '2019-01-22 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'EUR', 'Europe/Lisbon', 'pt-PT', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Advertising
('550e8400-e29b-41d4-a716-446655440046', 'Creative Ads Agency', 'ADS001', '43-5678902', 'REG-CZ-001', 'hello@creativeads.cz', '+420-2-1234-5678', 'https://www.creativeads.cz',
 'Václavské náměstí 1', NULL, 'Prague', 'Prague', '110 00', 'Czech Republic', 'Advertising',
 '2018-08-17 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'CZK', 'Europe/Prague', 'cs-CZ', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Transportation
('550e8400-e29b-41d4-a716-446655440047', 'Metro Transit Solutions', 'TRANS001', '53-6789013', 'REG-TR-001', 'info@metrotransit.com.tr', '+90-212-123-4567', 'https://www.metrotransit.com.tr',
 'İstiklal Caddesi 1', 'Beyoğlu', 'Istanbul', 'Istanbul', '34433', 'Turkey', 'Transportation',
 '2017-10-05 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Enterprise', 'TRY', 'Europe/Istanbul', 'tr-TR', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Biotechnology
('550e8400-e29b-41d4-a716-446655440048', 'BioGenetics Research', 'BIO001', '63-7890124', 'REG-RO-001', 'contact@biogenetics.ro', '+40-21-123-4567', 'https://www.biogenetics.ro',
 'Calea Victoriei 1', NULL, 'Bucharest', 'Bucharest', '010061', 'Romania', 'Biotechnology',
 '2019-09-14 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Professional', 'RON', 'Europe/Bucharest', 'ro-RO', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Fashion & Apparel
('550e8400-e29b-41d4-a716-446655440049', 'TrendSetter Fashion', 'TREND001', '73-8901235', 'REG-GR-001', 'info@trendsetter.gr', '+30-210-123-4567', 'https://www.trendsetter.gr',
 'Ermou 1', NULL, 'Athens', 'Attica', '105 63', 'Greece', 'Fashion',
 '2018-03-25 00:00:00+00', true, '2024-01-01 00:00:00+00', '2025-12-31 00:00:00+00',
 'Starter', 'EUR', 'Europe/Athens', 'el-GR', NULL, NOW(), NOW(),
 'system', NULL, NULL, NULL, false),

-- Software as a Service (Inactive company for testing)
('550e8400-e29b-41d4-a716-446655440050', 'CloudSoft Inactive Corp', 'INACTIVE001', '83-9012346', 'REG-HU-001', 'old@cloudsoft.hu', '+36-1-123-4567', 'https://www.cloudsoft.hu',
 'Andrássy út 1', NULL, 'Budapest', 'Budapest', '1061', 'Hungary', 'Technology',
 '2016-01-10 00:00:00+00', false, '2023-01-01 00:00:00+00', '2023-12-31 00:00:00+00',
 'Starter', 'HUF', 'Europe/Budapest', 'hu-HU', NULL, '2024-01-01 00:00:00+00', NULL,
 'system', NULL, NULL, NULL, false);

-- Verify insertion
SELECT COUNT(*) as total_companies FROM companies;

-- Sample queries to test
-- SELECT * FROM companies WHERE is_active = true;
-- SELECT * FROM companies WHERE country = 'USA';
-- SELECT * FROM companies WHERE industry = 'Technology';
-- SELECT * FROM companies WHERE subscription_plan = 'Enterprise';
-- SELECT * FROM companies WHERE currency = 'USD';
-- SELECT * FROM companies ORDER BY created_at DESC LIMIT 10;

