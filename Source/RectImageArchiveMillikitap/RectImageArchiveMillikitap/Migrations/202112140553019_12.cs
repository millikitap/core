namespace RectImageArchive.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _12 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TranslateLanguages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NameOnPage = c.String(),
                        Rus = c.String(),
                        Eng = c.String(),
                        Ar = c.String(),
                        Tat = c.String(),
                        OnPageId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TranslateLanguages");
        }
    }
}
