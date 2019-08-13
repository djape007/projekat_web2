namespace WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Buses",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        X = c.Double(nullable: false),
                        Y = c.Double(nullable: false),
                        LineId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lines", t => t.LineId)
                .Index(t => t.LineId);
            
            CreateTable(
                "dbo.Lines",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Direction = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.BusStopsOnLines",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        BusStopId = c.Guid(nullable: false),
                        LineId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BusStops", t => t.BusStopId, cascadeDelete: true)
                .ForeignKey("dbo.Lines", t => t.LineId, cascadeDelete: true)
                .Index(t => t.BusStopId)
                .Index(t => t.LineId);
            
            CreateTable(
                "dbo.BusStops",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        X = c.Double(nullable: false),
                        Y = c.Double(nullable: false),
                        Name = c.String(),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PointPathLines",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        X = c.Double(nullable: false),
                        Y = c.Double(nullable: false),
                        SequenceNumber = c.Int(nullable: false),
                        LineId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lines", t => t.LineId)
                .Index(t => t.LineId);
            
            CreateTable(
                "dbo.Coefficients",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(),
                        Value = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PriceHistories",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        PricelistId = c.Guid(nullable: false),
                        ProductTypeId = c.Guid(nullable: false),
                        Price = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Pricelists", t => t.PricelistId, cascadeDelete: true)
                .ForeignKey("dbo.ProductTypes", t => t.ProductTypeId, cascadeDelete: true)
                .Index(t => t.PricelistId)
                .Index(t => t.ProductTypeId);
            
            CreateTable(
                "dbo.Pricelists",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        From = c.DateTime(nullable: false),
                        To = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ProductTypes",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(nullable: false),
                        ExpiresAfterHours = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.SoldTickets",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.String(nullable: false),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Expires = c.DateTime(nullable: false),
                        Usages = c.Int(nullable: false),
                        Price = c.Double(nullable: false),
                        DateOfPurchase = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Status = c.String(),
                        Type = c.String(),
                        Password = c.String(),
                        Name = c.String(),
                        Surname = c.String(),
                        DateOfBirth = c.DateTime(nullable: false),
                        Address = c.String(),
                        HasDocument = c.Boolean(nullable: false),
                        Files = c.String(),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Timetables",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Times = c.String(),
                        ValidFrom = c.DateTime(nullable: false),
                        LineId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Lines", t => t.LineId)
                .Index(t => t.LineId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Timetables", "LineId", "dbo.Lines");
            DropForeignKey("dbo.SoldTickets", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.PriceHistories", "ProductTypeId", "dbo.ProductTypes");
            DropForeignKey("dbo.PriceHistories", "PricelistId", "dbo.Pricelists");
            DropForeignKey("dbo.PointPathLines", "LineId", "dbo.Lines");
            DropForeignKey("dbo.BusStopsOnLines", "LineId", "dbo.Lines");
            DropForeignKey("dbo.BusStopsOnLines", "BusStopId", "dbo.BusStops");
            DropForeignKey("dbo.Buses", "LineId", "dbo.Lines");
            DropIndex("dbo.Timetables", new[] { "LineId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.SoldTickets", new[] { "UserId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.PriceHistories", new[] { "ProductTypeId" });
            DropIndex("dbo.PriceHistories", new[] { "PricelistId" });
            DropIndex("dbo.PointPathLines", new[] { "LineId" });
            DropIndex("dbo.BusStopsOnLines", new[] { "LineId" });
            DropIndex("dbo.BusStopsOnLines", new[] { "BusStopId" });
            DropIndex("dbo.Buses", new[] { "LineId" });
            DropTable("dbo.Timetables");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.SoldTickets");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.ProductTypes");
            DropTable("dbo.Pricelists");
            DropTable("dbo.PriceHistories");
            DropTable("dbo.Coefficients");
            DropTable("dbo.PointPathLines");
            DropTable("dbo.BusStops");
            DropTable("dbo.BusStopsOnLines");
            DropTable("dbo.Lines");
            DropTable("dbo.Buses");
        }
    }
}
