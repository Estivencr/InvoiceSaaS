-- ============================================================
-- InvoiceSaaS - Seed Data
-- ============================================================
BEGIN;

-- ── Company ──────────────────────────────────────────────────
INSERT INTO companies (id, name, email, phone, document, address, country, subscription_plan, is_active)
VALUES (
  '11111111-1111-1111-1111-111111111111',
  'Empresa Demo S.A.S.',
  'contacto@empresademo.com',
  '+57 601 123 4567',
  '900123456-7',
  'Calle 123 # 45-67, Bogotá',
  'Colombia',
  'professional',
  true
);

-- ── Roles ────────────────────────────────────────────────────
INSERT INTO roles (id, company_id, name, description, is_custom) VALUES
  ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '11111111-1111-1111-1111-111111111111', 'Admin',    'Administrador con acceso total',  false),
  ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '11111111-1111-1111-1111-111111111111', 'Manager',  'Gestor con acceso a operaciones', false),
  ('cccccccc-cccc-cccc-cccc-cccccccccccc', '11111111-1111-1111-1111-111111111111', 'Employee', 'Empleado con acceso de lectura',  false);

-- ── Users ────────────────────────────────────────────────────
INSERT INTO users (id, company_id, email, password_hash, first_name, last_name, is_active) VALUES
  ('dddddddd-dddd-dddd-dddd-dddddddddddd', '11111111-1111-1111-1111-111111111111',
   'admin@demo.com',
   '$2a$10$W5eNp9BLcuRfcLfb3i62qOQFEGYsk3mCtTkK/Z/ryXw2i2nVKMOlq',
   'Carlos', 'Ramírez', true),
  ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', '11111111-1111-1111-1111-111111111111',
   'manager@demo.com',
   '$2a$10$XmMPSyysPv1r9eYdxJeLmuaTDv/OAZGkccQ7anOGwMTwKDjOwjBIy',
   'María', 'García', true);

-- ── User Roles ────────────────────────────────────────────────
INSERT INTO user_roles (id, user_id, role_id) VALUES
  (gen_random_uuid(), 'dddddddd-dddd-dddd-dddd-dddddddddddd', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'),
  (gen_random_uuid(), 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb');

-- ── Customers ─────────────────────────────────────────────────
INSERT INTO customers (id, company_id, name, document, phone, email, address, city, country, status, created_by) VALUES
  ('f1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111',
   'Comercializadora Andina S.A.', '800456789-1', '+57 310 111 2233', 'ventas@comercandina.com',
   'Av. El Dorado # 68-11', 'Bogotá', 'Colombia', 'active', 'dddddddd-dddd-dddd-dddd-dddddddddddd'),
  ('f2222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111',
   'Tecnología Global Ltda.', '830234567-2', '+57 311 222 3344', 'info@tecglobal.co',
   'Cra 11 # 93-21', 'Bogotá', 'Colombia', 'active', 'dddddddd-dddd-dddd-dddd-dddddddddddd'),
  ('f3333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111',
   'Constructora del Valle S.A.S.', '860345678-3', '+57 312 333 4455', 'contacto@convalle.com',
   'Calle 5 # 38-45', 'Cali', 'Colombia', 'active', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'),
  ('f4444444-4444-4444-4444-444444444444', '11111111-1111-1111-1111-111111111111',
   'Distribuidora Norte E.U.', '890456789-4', '+57 313 444 5566', 'compras@disnorte.com',
   'Av. Circunvalar # 12-34', 'Barranquilla', 'Colombia', 'active', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee'),
  ('f5555555-5555-5555-5555-555555555555', '11111111-1111-1111-1111-111111111111',
   'Inversiones Medellín S.A.', '811567890-5', '+57 314 555 6677', 'gerencia@invmed.com',
   'El Poblado # 43-32', 'Medellín', 'Colombia', 'inactive', 'dddddddd-dddd-dddd-dddd-dddddddddddd');

-- ── Products ──────────────────────────────────────────────────
INSERT INTO products (id, company_id, name, description, sku, category, unit_price, stock, unit, is_active, created_at, updated_at) VALUES
  ('a1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111',
   'Consultoría IT', 'Hora de consultoría en tecnología de la información',
   'CONS-IT-001', 'Servicios', 250000, 999, 'hora', true, NOW(), NOW()),
  ('a2222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111',
   'Desarrollo de Software', 'Desarrollo de aplicaciones a medida',
   'DEV-SW-001', 'Servicios', 200000, 999, 'hora', true, NOW(), NOW()),
  ('a3333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111',
   'Soporte Técnico', 'Soporte y mantenimiento de sistemas',
   'SOP-TEC-001', 'Servicios', 120000, 999, 'hora', true, NOW(), NOW()),
  ('a4444444-4444-4444-4444-444444444444', '11111111-1111-1111-1111-111111111111',
   'Licencia Software Enterprise', 'Licencia anual de software empresarial',
   'LIC-ENT-001', 'Licencias', 2000000, 50, 'unidad', true, NOW(), NOW()),
  ('a5555555-5555-5555-5555-555555555555', '11111111-1111-1111-1111-111111111111',
   'Capacitación', 'Sesión de capacitación grupal (hasta 10 personas)',
   'CAP-GRP-001', 'Capacitación', 750000, 999, 'sesión', true, NOW(), NOW());

-- ── Employees ─────────────────────────────────────────────────
INSERT INTO employees (id, company_id, name, email, position, role_id, hire_date, status) VALUES
  ('e1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111',
   'Carlos Ramírez', 'carlos.ramirez@empresademo.com', 'Gerente de TI',
   'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', '2024-01-15 00:00:00+00', 'active'),
  ('e2222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111',
   'María García', 'maria.garcia@empresademo.com', 'Analista de Negocios',
   'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', '2024-03-01 00:00:00+00', 'active'),
  ('e3333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111',
   'Andrés López', 'andres.lopez@empresademo.com', 'Desarrollador Senior',
   'cccccccc-cccc-cccc-cccc-cccccccccccc', '2024-06-15 00:00:00+00', 'active'),
  ('e4444444-4444-4444-4444-444444444444', '11111111-1111-1111-1111-111111111111',
   'Laura Torres', 'laura.torres@empresademo.com', 'Diseñadora UX',
   'cccccccc-cccc-cccc-cccc-cccccccccccc', '2025-02-01 00:00:00+00', 'active'),
  ('e5555555-5555-5555-5555-555555555555', '11111111-1111-1111-1111-111111111111',
   'Juan Morales', 'juan.morales@empresademo.com', 'Contador',
   'cccccccc-cccc-cccc-cccc-cccccccccccc', '2023-09-01 00:00:00+00', 'inactive');

-- ── Invoices ──────────────────────────────────────────────────
-- Abril 2026 - #1 (Pagada): Consultoría + Soporte
INSERT INTO invoices (id, company_id, invoice_number, customer_id, created_by,
  issue_date, due_date, subtotal, tax_rate, tax_amount, total, status, payment_date, notes)
VALUES (
  'b1111111-1111-1111-1111-111111111111', '11111111-1111-1111-1111-111111111111',
  'INV-2026-04-00001', 'f1111111-1111-1111-1111-111111111111', 'dddddddd-dddd-dddd-dddd-dddddddddddd',
  '2026-04-05 08:00:00+00', '2026-04-20 00:00:00+00',
  5500000, 19, 1045000, 6545000, 'Paid', '2026-04-15 10:30:00+00',
  'Pago recibido por transferencia bancaria'
);
INSERT INTO invoice_details (id, invoice_id, description, quantity, unit_price, amount, sequence) VALUES
  (gen_random_uuid(), 'b1111111-1111-1111-1111-111111111111', 'Consultoría IT - Implementación ERP', 20, 250000, 5000000, 1),
  (gen_random_uuid(), 'b1111111-1111-1111-1111-111111111111', 'Soporte técnico - Configuración de red',  5, 100000,  500000, 2);

-- Abril 2026 - #2 (Pagada): Desarrollo software
INSERT INTO invoices (id, company_id, invoice_number, customer_id, created_by,
  issue_date, due_date, subtotal, tax_rate, tax_amount, total, status, payment_date)
VALUES (
  'b2222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111',
  'INV-2026-04-00002', 'f2222222-2222-2222-2222-222222222222', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
  '2026-04-12 09:00:00+00', '2026-04-27 00:00:00+00',
  8000000, 19, 1520000, 9520000, 'Paid', '2026-04-25 14:00:00+00'
);
INSERT INTO invoice_details (id, invoice_id, description, quantity, unit_price, amount, sequence) VALUES
  (gen_random_uuid(), 'b2222222-2222-2222-2222-222222222222', 'Desarrollo de Software - Módulo de facturación', 40, 200000, 8000000, 1);

-- Abril 2026 - #3 (Pagada): Licencia + Capacitación
INSERT INTO invoices (id, company_id, invoice_number, customer_id, created_by,
  issue_date, due_date, subtotal, tax_rate, tax_amount, total, status, payment_date)
VALUES (
  'b3333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111',
  'INV-2026-04-00003', 'f3333333-3333-3333-3333-333333333333', 'dddddddd-dddd-dddd-dddd-dddddddddddd',
  '2026-04-18 10:00:00+00', '2026-05-03 00:00:00+00',
  3500000, 19, 665000, 4165000, 'Paid', '2026-04-30 11:00:00+00'
);
INSERT INTO invoice_details (id, invoice_id, description, quantity, unit_price, amount, sequence) VALUES
  (gen_random_uuid(), 'b3333333-3333-3333-3333-333333333333', 'Licencia Software Enterprise', 1, 2000000, 2000000, 1),
  (gen_random_uuid(), 'b3333333-3333-3333-3333-333333333333', 'Capacitación - Uso del sistema',   2,  750000, 1500000, 2);

-- Mayo 2026 - #1 (Pagada): Mantenimiento mensual
INSERT INTO invoices (id, company_id, invoice_number, customer_id, created_by,
  issue_date, due_date, subtotal, tax_rate, tax_amount, total, status, payment_date)
VALUES (
  'b4444444-4444-4444-4444-444444444444', '11111111-1111-1111-1111-111111111111',
  'INV-2026-05-00001', 'f1111111-1111-1111-1111-111111111111', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
  '2026-05-02 08:00:00+00', '2026-05-17 00:00:00+00',
  1200000, 19, 228000, 1428000, 'Paid', '2026-05-10 09:15:00+00'
);
INSERT INTO invoice_details (id, invoice_id, description, quantity, unit_price, amount, sequence) VALUES
  (gen_random_uuid(), 'b4444444-4444-4444-4444-444444444444', 'Mantenimiento mensual de sistemas', 1, 1200000, 1200000, 1);

-- Mayo 2026 - #2 (Pendiente): Auditoría + API + Capacitación
INSERT INTO invoices (id, company_id, invoice_number, customer_id, created_by,
  issue_date, due_date, subtotal, tax_rate, tax_amount, total, status, notes)
VALUES (
  'b5555555-5555-5555-5555-555555555555', '11111111-1111-1111-1111-111111111111',
  'INV-2026-05-00002', 'f4444444-4444-4444-4444-444444444444', 'dddddddd-dddd-dddd-dddd-dddddddddddd',
  '2026-05-15 11:00:00+00', '2026-05-30 00:00:00+00',
  4250000, 19, 807500, 5057500, 'Pending', 'Pendiente de aprobación por gerencia'
);
INSERT INTO invoice_details (id, invoice_id, description, quantity, unit_price, amount, sequence) VALUES
  (gen_random_uuid(), 'b5555555-5555-5555-5555-555555555555', 'Consultoría IT - Auditoría de seguridad', 10, 250000, 2500000, 1),
  (gen_random_uuid(), 'b5555555-5555-5555-5555-555555555555', 'Desarrollo de Software - API REST',       8, 200000, 1600000, 2),
  (gen_random_uuid(), 'b5555555-5555-5555-5555-555555555555', 'Capacitación - Seguridad informática',    2,  75000,  150000, 3);

-- Mayo 2026 - #3 (Pendiente): Consultoría estratégica
INSERT INTO invoices (id, company_id, invoice_number, customer_id, created_by,
  issue_date, due_date, subtotal, tax_rate, tax_amount, total, status)
VALUES (
  'b6666666-6666-6666-6666-666666666666', '11111111-1111-1111-1111-111111111111',
  'INV-2026-05-00003', 'f2222222-2222-2222-2222-222222222222', 'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
  '2026-05-20 14:00:00+00', '2026-06-04 00:00:00+00',
  3750000, 19, 712500, 4462500, 'Pending'
);
INSERT INTO invoice_details (id, invoice_id, description, quantity, unit_price, amount, sequence) VALUES
  (gen_random_uuid(), 'b6666666-6666-6666-6666-666666666666', 'Consultoría IT - Transformación digital', 15, 250000, 3750000, 1);

-- Mayo 2026 - #4 (Cancelada): Diseño web
INSERT INTO invoices (id, company_id, invoice_number, customer_id, created_by,
  issue_date, due_date, subtotal, tax_rate, tax_amount, total, status, notes)
VALUES (
  'b7777777-7777-7777-7777-777777777777', '11111111-1111-1111-1111-111111111111',
  'INV-2026-05-00004', 'f5555555-5555-5555-5555-555555555555', 'dddddddd-dddd-dddd-dddd-dddddddddddd',
  '2026-05-10 14:00:00+00', NULL,
  3000000, 19, 570000, 3570000, 'Cancelled', 'Cancelado por solicitud del cliente'
);
INSERT INTO invoice_details (id, invoice_id, description, quantity, unit_price, amount, sequence) VALUES
  (gen_random_uuid(), 'b7777777-7777-7777-7777-777777777777', 'Diseño y desarrollo web corporativo', 15, 200000, 3000000, 1);

COMMIT;
