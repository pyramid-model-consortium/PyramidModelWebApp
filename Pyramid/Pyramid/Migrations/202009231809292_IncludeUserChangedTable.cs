namespace Pyramid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncludeUserChangedTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AspNetUserChanges",
                c => new
                    {
                        AspNetUserChangePK = c.Int(nullable: false, identity: true),
                        ChangeDatetime = c.DateTime(nullable: false),
                        ChangeType = c.String(nullable: false),
                        Id = c.String(nullable: false),
                        FirstName = c.String(nullable: false),
                        LastName = c.String(nullable: false),
                        UpdateTime = c.DateTime(),
                        Email = c.String(),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false),
                        AccountEnabled = c.Boolean(nullable: false),
                        CreatedBy = c.String(nullable: false),
                        CreateTime = c.DateTime(nullable: false),
                        UpdatedBy = c.String(),
                    })
                .PrimaryKey(t => t.AspNetUserChangePK);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AspNetUserChanges");
        }
    }
}
