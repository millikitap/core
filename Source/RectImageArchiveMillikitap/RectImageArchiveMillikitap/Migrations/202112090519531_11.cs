namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _11 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attributes", "isExrimistStatements", c => c.Boolean());
            AddColumn("dbo.Attributes", "exrimistStatementsText", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Attributes", "exrimistStatementsText");
            DropColumn("dbo.Attributes", "isExrimistStatements");
        }
    }
}
