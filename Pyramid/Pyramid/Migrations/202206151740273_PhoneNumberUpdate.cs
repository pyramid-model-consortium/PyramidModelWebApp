namespace Pyramid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PhoneNumberUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUserChanges", "WorkPhoneNumber", c => c.String(maxLength: 40));
            AddColumn("dbo.AspNetUsers", "WorkPhoneNumber", c => c.String(maxLength: 40));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "WorkPhoneNumber");
            DropColumn("dbo.AspNetUserChanges", "WorkPhoneNumber");
        }
    }
}
