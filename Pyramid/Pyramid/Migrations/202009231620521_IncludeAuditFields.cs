namespace Pyramid.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncludeAuditFields : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "CreatedBy", c => c.String(nullable: false, defaultValue: "rowcreated"));
            AddColumn("dbo.AspNetUsers", "CreateTime", c => c.DateTime(nullable: false, defaultValueSql: "GetDate()"));
            AddColumn("dbo.AspNetUsers", "UpdatedBy", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "UpdatedBy");
            DropColumn("dbo.AspNetUsers", "CreateTime");
            DropColumn("dbo.AspNetUsers", "CreatedBy");
        }
    }
}
