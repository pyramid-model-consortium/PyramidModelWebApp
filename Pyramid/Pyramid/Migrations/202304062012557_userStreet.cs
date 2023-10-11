namespace Pyramid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class userStreet : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUserChanges", "Street", c => c.String(maxLength: 300));
            AddColumn("dbo.AspNetUsers", "Street", c => c.String(maxLength: 300));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "Street");
            DropColumn("dbo.AspNetUserChanges", "Street");
        }
    }
}
