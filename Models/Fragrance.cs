namespace WebAppComp3011.Models
{
    public class Fragrance
    {
        // nullable type??
        // probably need to make objects from the main accords - it's a list
        // check out perfume notes tooo

        /*
         BEGIN TRANSACTION;
CREATE TABLE IF NOT EXISTS "perfumeData" (
	"url"	TEXT,
	"Perfume"	TEXT,
	"Brand"	TEXT,
	"Country"	TEXT,
	"Gender"	TEXT,
	"Rating Value"	TEXT,
	"Rating Count"	INTEGER,
	"Year"	TEXT,
	"Top"	TEXT,
	"Middle"	TEXT,
	"Base"	TEXT,
	"Perfumer1"	TEXT,
	"Perfumer2"	TEXT,
	"mainaccord1"	TEXT,
	"mainaccord2"	TEXT,
	"mainaccord3"	TEXT,
	"mainaccord4"	TEXT,
	"mainaccord5"	TEXT
);
         */

        public int Id { get; set; }
        public string FragUrl { get; set; } // could be identifying
        public string FragName { get; set; }
        public string Brand { get; set; }
        public string Country { get; set; }
        public string Gender { get; set; }
        public float RatingValue { get; set; }
        public int RatingCount { get; set; }
        public string Year { get; set; }

        public Notes Notes { get; set; }
        
        // huuuh
        public string Perfumer1 { get; set; }
        public string Perfumer2 { get; set; }
        public List<String> MainAccords { get; set; }


    }

    public class Notes
    {
        public string Top { get; set; }
        public string Middle { get; set; }
        public string Base { get; set; }
    }
}
