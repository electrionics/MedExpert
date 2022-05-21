CREATE TABLE [dbo].[_AspNetUser](
    [Id] [nvarchar](50) NOT NULL,
    [UserName] [text] NULL,
    [NormalizedUserName] [nvarchar](100) NULL,
    [Email] [text] NULL,
    [NormalizedEmail] [nvarchar](100) NULL,
    [EmailConfirmed] [bit] NOT NULL,
    [PasswordHash] [text] NULL,
    [SecurityStamp] [text] NULL,
    [ConcurrencyStamp] [text] NULL,
    [PhoneNumber] [text] NULL,
    [PhoneNumberConfirmed] [bit] NOT NULL,
    [TwoFactorEnabled] [bit] NOT NULL,
    [LockoutEnd] [datetimeoffset](7) NULL,
    [LockoutEnabled] [bit] NOT NULL,
    [AccessFailedCount] [int] NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    GO



CREATE TABLE [dbo].[_AspNetRole](
    [Id] [nvarchar](50) NOT NULL,
    [Name] [text] NULL,
    [NormalizedName] [nvarchar](100) NULL,
    [ConcurrencyStamp] [text] NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    GO



CREATE TABLE [dbo].[_AspNetRoleClaim](
    [Id] [int] NOT NULL,
    [RoleId] [nvarchar](50) NOT NULL,
    [ClaimType] [text] NULL,
    [ClaimValue] [text] NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    GO

ALTER TABLE [dbo].[_AspNetRoleClaim]  WITH CHECK ADD  CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
    REFERENCES [dbo].[_AspNetRole] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[_AspNetRoleClaim] CHECK CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId]
    GO



CREATE TABLE [dbo].[_AspNetUserRole](
    [UserId] [nvarchar](50) NOT NULL,
    [RoleId] [nvarchar](50) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED
(
    [UserId] ASC,
[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY]
    GO

ALTER TABLE [dbo].[_AspNetUserRole]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY([RoleId])
    REFERENCES [dbo].[_AspNetRole] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[_AspNetUserRole] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId]
    GO

ALTER TABLE [dbo].[_AspNetUserRole]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[_AspNetUser] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[_AspNetUserRole] CHECK CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId]
    GO



CREATE TABLE [dbo].[_AspNetUserClaim](
    [Id] [int] NOT NULL,
    [UserId] [nvarchar](50) NOT NULL,
    [ClaimType] [text] NULL,
    [ClaimValue] [text] NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED
(
[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    GO

ALTER TABLE [dbo].[_AspNetUserClaim]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[_AspNetUser] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[_AspNetUserClaim] CHECK CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId]
    GO



CREATE TABLE [dbo].[_AspNetUserLogin](
    [LoginProvider] [nvarchar](100) NOT NULL,
    [ProviderKey] [nvarchar](100) NOT NULL,
    [ProviderDisplayName] [text] NULL,
    [UserId] [nvarchar](50) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED
(
    [LoginProvider] ASC,
[ProviderKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    GO

ALTER TABLE [dbo].[_AspNetUserLogin]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[_AspNetUser] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[_AspNetUserLogin] CHECK CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId]
    GO



CREATE TABLE [dbo].[_AspNetUserToken](
    [UserId] [nvarchar](50) NOT NULL,
    [LoginProvider] [nvarchar](100) NOT NULL,
    [Name] [nvarchar](50) NOT NULL,
    [Value] [text] NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED
(
    [UserId] ASC,
    [LoginProvider] ASC,
[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
    GO

ALTER TABLE [dbo].[_AspNetUserToken]  WITH CHECK ADD  CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[_AspNetUser] ([Id])
    ON DELETE CASCADE
GO

ALTER TABLE [dbo].[_AspNetUserToken] CHECK CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId]
    GO