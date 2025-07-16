--create database orchid_shop_db

CREATE TABLE "orchids"(
    "id" uniqueidentifier NOT NULL,
    "name" NVARCHAR(255) NOT NULL,
    "description" TEXT NULL,
    "url" TEXT NULL,
    "price" DECIMAL(8, 2) NOT NULL,
    "is_natural" BIT NOT NULL,
    "category_id" uniqueidentifier NOT NULL
);
ALTER TABLE
    "orchids" ADD CONSTRAINT "orchids_id_primary" PRIMARY KEY("id");
CREATE TABLE "categories"(
    "id" uniqueidentifier NOT NULL,
    "name" NVARCHAR(255) NOT NULL
);
ALTER TABLE
    "categories" ADD CONSTRAINT "categories_id_primary" PRIMARY KEY("id");
CREATE TABLE "orders"(
    "id" uniqueidentifier NOT NULL,
    "total_amount" DECIMAL(8, 2) NOT NULL,
    "order_date" DATE NOT NULL,
    "account_id" uniqueidentifier NOT NULL,
    "status" VARCHAR(255) NOT NULL
);
ALTER TABLE
    "orders" ADD CONSTRAINT "orders_id_primary" PRIMARY KEY("id");
CREATE TABLE "order_details"(
    "id" uniqueidentifier NOT NULL,
    "price" DECIMAL(8, 2) NOT NULL,
    "quantity" INT NOT NULL,
    "orchid_id" uniqueidentifier NOT NULL,
    "order_id" uniqueidentifier NOT NULL
);
ALTER TABLE
    "order_details" ADD CONSTRAINT "order_details_id_primary" PRIMARY KEY("id");
CREATE TABLE "accounts"(
    "id" uniqueidentifier NOT NULL,
    "name" NVARCHAR(255) NULL,
    "email" NVARCHAR(255) NOT NULL,
    "password" NVARCHAR(255) NOT NULL,
    "role_id" uniqueidentifier NOT NULL
);
ALTER TABLE
    "accounts" ADD CONSTRAINT "accounts_id_primary" PRIMARY KEY("id");
CREATE TABLE "roles"(
    "id" uniqueidentifier NOT NULL,
    "name" VARCHAR(255) NOT NULL
);
ALTER TABLE
    "roles" ADD CONSTRAINT "roles_id_primary" PRIMARY KEY("id");

-- Fixed Foreign Key Constraints with correct relationships and unique names
ALTER TABLE
    "accounts" ADD CONSTRAINT "accounts_role_id_foreign" FOREIGN KEY("role_id") REFERENCES "roles"("id");
ALTER TABLE
    "orders" ADD CONSTRAINT "orders_account_id_foreign" FOREIGN KEY("account_id") REFERENCES "accounts"("id");
ALTER TABLE
    "order_details" ADD CONSTRAINT "order_details_order_id_foreign" FOREIGN KEY("order_id") REFERENCES "orders"("id");
ALTER TABLE
    "orchids" ADD CONSTRAINT "orchids_category_id_foreign" FOREIGN KEY("category_id") REFERENCES "categories"("id");
ALTER TABLE
    "order_details" ADD CONSTRAINT "order_details_orchid_id_foreign" FOREIGN KEY("orchid_id") REFERENCES "orchids"("id");