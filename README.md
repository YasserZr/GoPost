# GoPost

A modern social media platform built with ASP.NET Core 7.0, featuring user authentication, posts, comments, reactions, and real-time notifications.

## 🚀 Features

- **User Authentication & Authorization**
  - ASP.NET Core Identity integration
  - Role-based access control (Admin & User roles)
  - Email confirmation support
  - Account management

- **Social Networking**
  - Create, edit, and delete posts with rich content
  - Upload images and file attachments
  - Like/Unlike posts
  - Comment on posts
  - Follow/Unfollow users
  - View user profiles

- **Admin Dashboard**
  - User management
  - Lock/Unlock user accounts
  - Change user roles
  - View all posts and users

- **Notifications System**
  - Real-time notifications for follows, comments, and reactions
  - Mark notifications as read
  - Notification count badges

- **Modern UI/UX**
  - Responsive design with Bootstrap 5
  - Custom styling with Poppins font
  - Interactive components
  - Smooth animations and transitions

## 🛠️ Technologies Used

- **Backend**: ASP.NET Core 7.0 MVC
- **Database**: SQL Server (configurable to SQL Server Express or LocalDB)
- **ORM**: Entity Framework Core 7.0
- **Authentication**: ASP.NET Core Identity
- **Frontend**: 
  - Razor Pages
  - Bootstrap 5
  - Bootstrap Icons
  - JavaScript/jQuery
  - Custom CSS

## 📋 Prerequisites

Before running this project, ensure you have the following installed:

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (optional, but recommended)
- [Git](https://git-scm.com/)

## ⚙️ Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/YasserZr/GoPost.git
cd GoPost
```

### 2. Configure Database Connection

Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=aspnet-GoPost;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

> **Note**: Adjust the connection string based on your SQL Server setup.

### 3. Install Dependencies

```bash
cd GoPost
dotnet restore
```

### 4. Apply Database Migrations

```bash
dotnet ef database update
```

This will create the database and apply all migrations.

### 5. Run the Application

```bash
dotnet run
```

The application will be available at:
- **HTTPS**: https://localhost:7134
- **HTTP**: http://localhost:5219

## 👤 Default Admin Account

After running the application, the system will attempt to create an admin role. To create an admin user:

1. Register a new account with email: `admin@example.com`
2. The system will automatically assign the Admin role if configured in `Program.cs`

Alternatively, update the email in `Program.cs` to match your desired admin account.

## 📁 Project Structure

```
GoPost/
├── Areas/
│   └── Identity/          # Identity pages (login, register, etc.)
├── Attributes/            # Custom attributes
├── Controllers/           # MVC Controllers
│   ├── AdminController.cs
│   ├── CommentsController.cs
│   ├── FollowsController.cs
│   ├── HomeController.cs
│   ├── NotificationsController.cs
│   ├── PostsController.cs
│   ├── ProfilesController.cs
│   └── ReactionsController.cs
├── Data/                  # Database context and migrations
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── Helpers/               # Helper classes
│   └── StringExtensions.cs
├── Models/                # Data models and ViewModels
│   ├── ApplicationUser.cs
│   ├── Post.cs
│   ├── Comment.cs
│   ├── Reaction.cs
│   ├── Follow.cs
│   ├── Notification.cs
│   └── ViewModels/
├── Views/                 # Razor views
│   ├── Home/
│   ├── Posts/
│   ├── Profiles/
│   ├── Admin/
│   └── Shared/
├── wwwroot/              # Static files (CSS, JS, images)
│   ├── css/
│   ├── js/
│   ├── images/
│   └── uploads/          # User-uploaded files
├── appsettings.json      # Application configuration
└── Program.cs            # Application entry point
```

## 🔧 Configuration

### Email Confirmation

To enable email confirmation, configure SMTP settings in `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "GoPost",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### Identity Options

Password requirements and other identity options can be configured in `Program.cs`:

```csharp
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = true;
})
```

## 📝 Usage

### Creating a Post

1. Log in to your account
2. Navigate to the home page
3. Click "Create New Post"
4. Fill in the title and content
5. Optionally upload images or files
6. Click "Create" to publish

### Following Users

1. Search for users using the search bar
2. Visit their profile
3. Click the "Follow" button
4. View their posts in your feed

### Admin Features

1. Log in with an admin account
2. Navigate to the Admin Dashboard
3. Manage users, lock/unlock accounts, and change roles

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💻 Author

**YasserZr**
- GitHub: [@YasserZr](https://github.com/YasserZr)

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Bootstrap team for the UI components
- All contributors and users of this project

## 📞 Support

If you have any questions or need help, please open an issue on GitHub.

---

Made with ❤️ using ASP.NET Core
