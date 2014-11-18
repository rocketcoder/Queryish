namespace Queryish.UnitTest.Repository.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestCases",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Score = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ScoreDate = c.DateTime(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            Sql("Insert Into TestCases(Name, Score, ScoreDate, Enabled) Values ('dude duderson', 66.7, '1/1/2014', 1);");
        }
        
        public override void Down()
        {
            DropTable("dbo.TestCases");
        }
    }
}
