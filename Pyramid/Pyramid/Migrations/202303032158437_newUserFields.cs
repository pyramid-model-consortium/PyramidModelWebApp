namespace Pyramid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newUserFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUserChanges", "MailingAddress", c => c.String(maxLength: 256));
            AddColumn("dbo.AspNetUserChanges", "RegionLocation", c => c.String(maxLength: 256));
            AddColumn("dbo.AspNetUsers", "MailingAddress", c => c.String(maxLength: 256));
            AddColumn("dbo.AspNetUsers", "RegionLocation", c => c.String(maxLength: 256));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "RegionLocation");
            DropColumn("dbo.AspNetUsers", "MailingAddress");
            DropColumn("dbo.AspNetUserChanges", "RegionLocation");
            DropColumn("dbo.AspNetUserChanges", "MailingAddress");
        }
    }
}
