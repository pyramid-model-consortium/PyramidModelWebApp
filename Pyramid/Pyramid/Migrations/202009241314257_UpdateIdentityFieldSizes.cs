namespace Pyramid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateIdentityFieldSizes : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AspNetUserChanges", "ChangeType", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.AspNetUserChanges", "Id", c => c.String(nullable: false, maxLength: 128));
            AlterColumn("dbo.AspNetUserChanges", "FirstName", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.AspNetUserChanges", "LastName", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.AspNetUserChanges", "Email", c => c.String(maxLength: 256));
            AlterColumn("dbo.AspNetUserChanges", "UserName", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.AspNetUserChanges", "CreatedBy", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.AspNetUserChanges", "UpdatedBy", c => c.String(maxLength: 256));
            AlterColumn("dbo.AspNetUsers", "FirstName", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.AspNetUsers", "LastName", c => c.String(nullable: false, maxLength: 512));
            AlterColumn("dbo.AspNetUsers", "CreatedBy", c => c.String(nullable: false, maxLength: 256));
            AlterColumn("dbo.AspNetUsers", "UpdatedBy", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AspNetUsers", "UpdatedBy", c => c.String());
            AlterColumn("dbo.AspNetUsers", "CreatedBy", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUsers", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUsers", "FirstName", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUserChanges", "UpdatedBy", c => c.String());
            AlterColumn("dbo.AspNetUserChanges", "CreatedBy", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUserChanges", "UserName", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUserChanges", "Email", c => c.String());
            AlterColumn("dbo.AspNetUserChanges", "LastName", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUserChanges", "FirstName", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUserChanges", "Id", c => c.String(nullable: false));
            AlterColumn("dbo.AspNetUserChanges", "ChangeType", c => c.String(nullable: false));
        }
    }
}
