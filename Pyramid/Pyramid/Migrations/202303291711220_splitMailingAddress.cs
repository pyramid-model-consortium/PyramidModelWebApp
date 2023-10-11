namespace Pyramid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class splitMailingAddress : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUserChanges", "ZIPCode", c => c.String(maxLength: 50));
            AddColumn("dbo.AspNetUserChanges", "City", c => c.String(maxLength: 100));
            AddColumn("dbo.AspNetUserChanges", "State", c => c.String(maxLength: 50));
            AddColumn("dbo.AspNetUsers", "ZIPCode", c => c.String(maxLength: 50));
            AddColumn("dbo.AspNetUsers", "City", c => c.String(maxLength: 100));
            AddColumn("dbo.AspNetUsers", "State", c => c.String(maxLength: 50));
            DropColumn("dbo.AspNetUserChanges", "MailingAddress");
            DropColumn("dbo.AspNetUsers", "MailingAddress");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "MailingAddress", c => c.String(maxLength: 256));
            AddColumn("dbo.AspNetUserChanges", "MailingAddress", c => c.String(maxLength: 256));
            DropColumn("dbo.AspNetUsers", "State");
            DropColumn("dbo.AspNetUsers", "City");
            DropColumn("dbo.AspNetUsers", "ZIPCode");
            DropColumn("dbo.AspNetUserChanges", "State");
            DropColumn("dbo.AspNetUserChanges", "City");
            DropColumn("dbo.AspNetUserChanges", "ZIPCode");
        }
    }
}
