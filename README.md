# Inventory Management System - Web API

A robust **ASP.NET Core Web API** backend for managing inventory with **JWT authentication**, **role-based authorization**, and **image upload** capabilities.


## Features

### Authentication & Authorization
- JWT Token-based authentication
- Role-based access control(Admin & User roles)
- Password hashing using ASP.NET Core Identity
- Secure token generation with 2-hour expiration

### Product Management
- CRUD operations for products
- Multiple image uploads per product
- Image validation (type & size)
- Pagination support (efficient data retrieval)
- Product search and filtering

### Technical Features
- RESTful API design
- Entity Framework Core with SQL Server
- Async/await for optimal performance
- CORS enabled for cross-origin requests
- Comprehensive logging
- Error handling & validation

---

## 🛠️ Tech Stack

| Technology | Version | Purpose |
|-----------|---------|---------|
| .NET | 8.0 | Framework |
| ASP.NET Core | 8.0 | Web API |
| Entity Framework Core | 8.0 | ORM |
| SQL Server | 2019+ | Database |
| JWT Bearer | 8.0 | Authentication |
| Swashbuckle | 6.5.0 | API Documentation |

---

## 📁 Project Structure
```
InventoryAPI/
├── Controllers/
│   ├── AuthController.cs        # Authentication endpoints
│   └── ProductController.cs     # Product CRUD endpoints
├── Data/
│   └── ApplicationDbContext.cs  # Database context
├── Models/
│   ├── User.cs                  # User entity
│   ├── Product.cs               # Product entity
│   ├── ProductImage.cs          # Product image entity
│   └── DTO/                     # Data Transfer Objects
├── Services/
│   ├── IJwtService.cs           # JWT interface
│   ├── JwtService.cs            # JWT implementation
│   ├── IProductService.cs       # Product service interface
│   └── ProductService.cs        # Product service implementation
├── wwwroot/
│   └── product-images/          # Uploaded product images
├── appsettings.json             # Configuration
└── Program.cs                   # Application entry point
```

---

##Installation & Setup

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB or Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Step 1: Clone the Repository
```bash
git clone https://github.com/YOUR-USERNAME/InventoryManagementAPI.git
cd InventoryManagementAPI
```

### Step 2: Update Connection String
Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=SimpleInventoryDB;Trusted_Connection=True;"
  }
}
```

### Step 3: Apply Database Migrations
```bash
dotnet ef database update
```

If migrations don't exist, create them:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Step 4: Run the Application
```bash
dotnet run
```

API will be available at: `https://localhost:7105`

---

## API Documentation

### Authentication Endpoints

#### Register User
```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "role": "User"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-12-13T14:30:00Z",
  "username": "john_doe",
  "role": "User"
}
```

#### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "john_doe",
  "password": "SecurePass123!"
}
```

---

### Product Endpoints

#### Get All Products (Public)
```http
GET /api/product/get?pageNumber=1&pageSize=8
```

**Response:**
```json
{
  "products": [
    {
      "id": 1,
      "name": "Gaming Laptop",
      "description": "High-performance laptop",
      "price": 1299.99,
      "quantity": 10,
      "images": [
        {
          "id": 1,
          "imageUrl": "/product-images/guid.jpg"
        }
      ]
    }
  ],
  "totalCount": 45,
  "totalPages": 6,
  "currentPage": 1,
  "pageSize": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

#### Get Product by ID (Public)
```http
GET /api/product/1
```

#### Create Product (Admin Only)
```http
POST /api/product/add
Authorization: Bearer {token}
Content-Type: multipart/form-data

Name: Gaming Laptop
Description: High-performance laptop
Price: 1299.99
Quantity: 10
Images: [file1.jpg, file2.jpg]
```

#### Update Product (Admin Only)
```http
PUT /api/product/1
Authorization: Bearer {token}
Content-Type: multipart/form-data

Name: Gaming Laptop Pro
Price: 1499.99
Quantity: 8
NewImages: [file3.jpg]
RemoveImageIds: [1, 2]
```

### Delete Product (Admin Only)
```http
DELETE /api/product/1
Authorization: Bearer {token}
```

---

## 🔐 Security Features

1. **JWT Authentication**
   - Tokens expire after 2 hours
   - Signed with HMAC-SHA256
   - Contains user ID, username, and role

2. **Password Security**
   - Hashed using ASP.NET Core Identity PasswordHasher
   - Includes salt to prevent rainbow table attacks

3. **Role-Based Authorization**
   - User: Can view products only
   - Admin: Full CRUD operations

4. **Input Validation**
   - Model validation attributes
   - File type validation (images only)
   - File size validation (max 5MB)

5. **CORS Protection**
   - Configured for specific origins only
   - Credentials allowed for authenticated requests

---

## 🎯 Key Endpoints Summary

| Method | Endpoint | Auth | Role | Description |
|--------|----------|------|------|-------------|
| POST | `/api/auth/register` | ❌ | - | Register new user |
| POST | `/api/auth/login` | ❌ | - | Login user |
| GET | `/api/product/get` | ❌ | - | Get all products (paginated) |
| GET | `/api/product/{id}` | ❌ | - | Get product details |
| POST | `/api/product/add` | ✅ | Admin | Create product |
| PUT | `/api/product/{id}` | ✅ | Admin | Update product |
| DELETE | `/api/product/{id}` | ✅ | Admin | Delete product |
| POST | `/api/product/{id}/images` | ✅ | Admin | Upload images |
| DELETE | `/api/product/images/{id}` | ✅ | Admin | Delete image |

---

## 🧪 Testing

### Using Postman

1. **Register Admin:**
```json
   POST https://localhost:7105/api/auth/register
   {
     "username": "admin",
     "email": "admin@test.com",
     "password": "Admin@123",
     "confirmPassword": "Admin@123",
     "role": "Admin"
   }
```

2. **Copy the token from response**

3. **Create Product:**
   - Method: POST
   - URL: `https://localhost:7105/api/product/add`
   - Headers: `Authorization: Bearer {token}`
   - Body: form-data with product details + images

---

## 🐛 Troubleshooting

### Database Connection Issues
- Ensure SQL Server is running
- Verify connection string in `appsettings.json`
- Run: `dotnet ef database update`

### JWT Token Errors
- Check token expiration (2 hours)
- Verify JWT secret key matches in `appsettings.json`
- Ensure role claim format is correct

### Image Upload Issues
- Check `wwwroot/product-images` folder exists
- Verify file size < 5MB
- Allowed formats: .jpg, .jpeg, .png, .gif, .webp

### CORS Errors
- Add your client URL to CORS policy in `Program.cs`
- Ensure credentials are enabled

---

## 📄 License

This project is created for educational purposes.

---

## 👨‍💻 Author

**Your Name**
- GitHub: [@your-username](https://github.com/your-username)
- Email: your.email@example.com

---

## 🙏 Acknowledgments

- ASP.NET Core Team
- Entity Framework Core Team
- JWT.io for token verification tools

---

## 📸 Screenshots

### API Running
![API Running](screenshots/api-running.png)

### Postman Testing
![Postman Test](screenshots/postman-test.png)

---

## 🔄 Version History

- **v1.0.0** (December 2025)
  - Initial release
  - JWT authentication
  - Product CRUD
  - Image upload
  - Pagination
