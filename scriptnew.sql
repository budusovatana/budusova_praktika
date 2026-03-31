\! chcp 1251

DO $$
BEGIN
   IF NOT EXISTS (SELECT 1 FROM pg_roles WHERE rolname = 'app') THEN
      CREATE ROLE app LOGIN PASSWORD '123456789';
   END IF;
END$$;

DROP DATABASE IF EXISTS budusova_db_partners;
CREATE DATABASE budusova_db_partners OWNER app;
\c budusova_db_partners

CREATE SCHEMA IF NOT EXISTS app AUTHORIZATION app;
ALTER ROLE app SET search_path TO app;
SET search_path TO app;

CREATE TABLE app.partner_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE app.partners (
    id SERIAL PRIMARY KEY,
    partner_type_id INT NOT NULL REFERENCES app.partner_types(id) ON DELETE RESTRICT,
    name VARCHAR(200) NOT NULL,
    legal_address VARCHAR(300) NOT NULL,
    inn VARCHAR(12),
    director_full_name VARCHAR(200) NOT NULL,
    phone VARCHAR(50),
    email VARCHAR(200),
    rating INT NOT NULL DEFAULT 0 CHECK (rating >= 0),
    sales_points VARCHAR(300),
    logo_path VARCHAR(300)
);

CREATE TABLE app.product_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE app.products (
    id SERIAL PRIMARY KEY,
    product_type_id INT NOT NULL REFERENCES app.product_types(id) ON DELETE RESTRICT,
    article VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(500),
    min_partner_price DECIMAL(18,2) NOT NULL CHECK (min_partner_price >= 0)
);

CREATE TABLE app.partner_sales (
    id SERIAL PRIMARY KEY,
    partner_id INT NOT NULL REFERENCES app.partners(id) ON DELETE CASCADE,
    product_id INT NOT NULL REFERENCES app.products(id) ON DELETE RESTRICT,
    quantity INT NOT NULL CHECK (quantity > 0),
    unit_price DECIMAL(18,2) NOT NULL CHECK (unit_price >= 0),
    sale_date DATE NOT NULL
);

CREATE INDEX idx_partners_partner_type_id ON app.partners(partner_type_id);
CREATE INDEX idx_products_product_type_id ON app.products(product_type_id);
CREATE INDEX idx_partner_sales_partner_id ON app.partner_sales(partner_id);
CREATE INDEX idx_partner_sales_product_id ON app.partner_sales(product_id);
CREATE INDEX idx_partner_sales_sale_date ON app.partner_sales_sale_date ON app.partner_sales(sale_date);

CREATE OR REPLACE PROCEDURE app.add_partner(
    p_partner_type_id INT,
    p_name VARCHAR,
    p_legal_address VARCHAR,
    p_director_full_name VARCHAR,
    p_phone VARCHAR,
    p_email VARCHAR,
    p_rating INT
)
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO app.partners(partner_type_id, name, legal_address, director_full_name, phone, email, rating)
    VALUES (p_partner_type_id, p_name, p_legal_address, p_director_full_name, p_phone, p_email, p_rating);
END$$;

CREATE OR REPLACE PROCEDURE app.add_partner_sale(
    p_partner_id INT,
    p_product_id INT,
    p_quantity INT,
    p_unit_price DECIMAL,
    p_sale_date DATE
)
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO app.partner_sales(partner_id, product_id, quantity, unit_price, sale_date)
    VALUES (p_partner_id, p_product_id, p_quantity, p_unit_price, p_sale_date);
END$$;

CREATE OR REPLACE FUNCTION app.partner_total_sales_amount(p_partner_id INT)
RETURNS DECIMAL(18,2) AS $$
    SELECT COALESCE(SUM(quantity * unit_price),0)
    FROM app.partner_sales
    WHERE partner_id = p_partner_id;
$$ LANGUAGE sql;

CREATE OR REPLACE FUNCTION app.partner_discount_percent(p_partner_id INT)
RETURNS INT AS $$
DECLARE total DECIMAL(18,2);
BEGIN
    SELECT COALESCE(SUM(quantity * unit_price),0) INTO total
    FROM app.partner_sales
    WHERE partner_id = p_partner_id;

    IF total < 10000 THEN RETURN 0;
    ELSIF total < 50000 THEN RETURN 5;
    ELSIF total < 300000 THEN RETURN 10;
    ELSE RETURN 15;
    END IF;
END$$ LANGUAGE plpgsql;

ALTER TABLE app.partner_types OWNER TO app;
ALTER TABLE app.partners OWNER TO app;
ALTER TABLE app.product_types OWNER TO app;
ALTER TABLE app.products OWNER TO app;
ALTER TABLE app.partner_sales OWNER TO app;

ALTER PROCEDURE app.add_partner(INT, VARCHAR, VARCHAR, VARCHAR, VARCHAR, VARCHAR, INT) OWNER TO app;
ALTER PROCEDURE app.add_partner_sale(INT, INT, INT, DECIMAL, DATE) OWNER TO app;
ALTER FUNCTION app.partner_total_sales_amount(INT) OWNER TO app;
ALTER FUNCTION app.partner_discount_percent(INT) OWNER TO app;

GRANT USAGE ON SCHEMA app TO app;
GRANT ALL ON ALL TABLES IN SCHEMA app TO app;
GRANT USAGE, SELECT, UPDATE ON ALL SEQUENCES IN SCHEMA app TO app;
GRANT EXECUTE ON ALL FUNCTIONS IN SCHEMA app TO app;
GRANT EXECUTE ON ALL PROCEDURES IN SCHEMA app TO app;

INSERT INTO app.partner_types(name) VALUES
('Дистрибьютор'),('Розничный магазин'),('Интернет-магазин'),('Оптовый партнер');

INSERT INTO app.product_types(name) VALUES
('Ламинат'),('Паркет'),('Плитка ПВХ'),('Линолеум');

INSERT INTO app.products(product_type_id, article, name, description, min_partner_price) VALUES
(1,'LAM-001','Ламинат Дуб Светлый','32 класс',1200.00),
(1,'LAM-002','Ламинат Орех Темный','33 класс',1450.00),
(2,'PAR-001','Паркет Классик','Натуральный паркет',3500.00),
(3,'PVC-001','Плитка ПВХ Standart','Влагостойкое покрытие',950.00),
(4,'LIN-001','Линолеум Comfort','Бытовой линолеум',700.00);

INSERT INTO app.partners(partner_type_id,name,legal_address,director_full_name,phone,email,rating) VALUES
(2,'Мир Плитки','г. Москва','Иванов И.И.','+7-900-111-22-33','info@mirplitki.ru',9),
(3,'СтройМаркет','г. Санкт-Петербург','Петров П.П.','+7-901-222-33-44','contact@stroymarket.ru',8),
(2,'ПолКомфорт','г. Казань','Сидоров С.С.','+7-902-333-44-55','support@polkomfort.ru',7),
(1,'ПлиткаПро','г. Новосибирск','Кузнецов К.К.','+7-903-444-55-66','sales@plitkapro.ru',10),
(2,'ДомСтрой','г. Екатеринбург','Федоров Ф.Ф.','+7-904-555-66-77','info@domstroy.ru',8),
(1,'ПлиткаЛэнд','г. Нижний Новгород','Александров А.А.','+7-905-666-77-88','contact@plitkalend.ru',9),
(2,'СтройСервис','г. Самара','Васильев В.В.','+7-906-777-88-99','service@stroiservice.ru',7),
(1,'ТопПлитка','г. Ростов-на-Дону','Смирнова С.С.','+7-907-888-99-00','top@topplitka.ru',10),
(2,'МегаСтрой','г. Уфа','Орлова О.О.','+7-908-999-00-11','mega@megastroy.ru',8),
(1,'ПлиткаЦентр','г. Волгоград','Павлов П.П.','+7-909-000-11-22','center@plitkacenter.ru',9),
(2,'ООО "Новый Партнер"','г. Пермь','Васильев О.Н.','+7-912-111-22-33','newpartner@mail.ru',5);

INSERT INTO app.partner_sales(partner_id, product_id, quantity, unit_price, sale_date) VALUES
-- 0%
(1,1,1200,1200,CURRENT_DATE),(1,2,800,1450,CURRENT_DATE),(1,3,700,3500,CURRENT_DATE),(1,4,600,700,CURRENT_DATE),(1,1,900,1200,CURRENT_DATE),
-- итого: 4200

(2,1,1500,1200,CURRENT_DATE),(2,2,1200,1450,CURRENT_DATE),(2,3,1100,3500,CURRENT_DATE),(2,4,900,700,CURRENT_DATE),(2,1,1300,1200,CURRENT_DATE),
-- итого: 6000

-- 5%
(3,1,2500,1200,CURRENT_DATE),(3,2,2200,1450,CURRENT_DATE),(3,3,2100,3500,CURRENT_DATE),(3,4,1800,700,CURRENT_DATE),(3,1,1700,1200,CURRENT_DATE),
-- итого: 10300

(4,1,3000,1200,CURRENT_DATE),(4,2,2500,1450,CURRENT_DATE),(4,3,2200,3500,CURRENT_DATE),(4,4,1700,700,CURRENT_DATE),(4,1,1600,1200,CURRENT_DATE),
-- итого: 11000

(5,1,3500,1200,CURRENT_DATE),(5,2,2800,1450,CURRENT_DATE),(5,3,2300,3500,CURRENT_DATE),(5,4,1900,700,CURRENT_DATE),(5,1,1700,1200,CURRENT_DATE),
-- итого: 12200

-- 10%
(6,1,12000,1200,CURRENT_DATE),(6,2,10000,1450,CURRENT_DATE),(6,3,9000,3500,CURRENT_DATE),(6,4,8000,700,CURRENT_DATE),(6,1,7000,1200,CURRENT_DATE),
-- итого: 46000

(7,1,13000,1200,CURRENT_DATE),(7,2,11000,1450,CURRENT_DATE),(7,3,10000,3500,CURRENT_DATE),(7,4,9000,700,CURRENT_DATE),(7,1,7000,1200,CURRENT_DATE),
-- итого: 50000 -> уже 10%

(8,1,15000,1200,CURRENT_DATE),(8,2,13000,1450,CURRENT_DATE),(8,3,11000,3500,CURRENT_DATE),(8,4,9000,700,CURRENT_DATE),(8,1,8000,1200,CURRENT_DATE),
-- итого: 56000

-- 15%
(9,1,70000,1200,CURRENT_DATE),(9,2,65000,1450,CURRENT_DATE),(9,3,62000,3500,CURRENT_DATE),(9,4,58000,700,CURRENT_DATE),(9,1,55000,1200,CURRENT_DATE),
-- итого: 310000

(10,1,80000,1200,CURRENT_DATE),(10,2,70000,1450,CURRENT_DATE),(10,3,65000,3500,CURRENT_DATE),(10,4,60000,700,CURRENT_DATE),(10,1,50000,1200,CURRENT_DATE),
-- итого: 325000

(11,1,90000,1200,CURRENT_DATE),(11,2,80000,1450,CURRENT_DATE),(11,3,70000,3500,CURRENT_DATE),(11,4,60000,700,CURRENT_DATE),(11,1,50000,1200,CURRENT_DATE);
-- итого: 350000