using System.Text.Json.Serialization;

namespace appImprimir
{
    public class MPrint
    {
        [JsonPropertyName("print")]
        public List<MDatos> Print {  get; set; }
    }

    public class MDatos
    {
        [JsonPropertyName("titulo")]
        public string Titulo { get; set; }
        [JsonPropertyName("valor")]
        public string Valor { get; set; }
    }
}
