namespace WebAppComp3011.Models
{
    public class Fragrance
    {
        // nullable type??
        // probably need to make objects from the main accords - it's a list
        // check out perfume notes tooo

        public int Id { get; set; }
        public string FragUrl { get; set; } // could be identifying
        public string FragName { get; set; }
        public string Brand { get; set; }
        public string Country { get; set; }
        public string Gender { get; set; }
        public float RatingValue { get; set; }
        public int RatingCount { get; set; }
        public string Year { get; set; }
        public string Top { get; set; }
        public string Middle { get; set; }
        public string Base { get; set; }
        
        // huuuh
        public string Perfumer1 { get; set; }
        public string Perfumer2 { get; set; }
        public List<String> MainAccords { get; set; }
        //public string MainAccord2 { get; set; }
        //public string MainAccord3 { get; set; }
        //public string MainAccord4 { get; set; }
        //public string MainAccord5 { get; set; }

    }
}
