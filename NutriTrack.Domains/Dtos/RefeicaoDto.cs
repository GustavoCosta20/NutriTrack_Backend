namespace NutriTrack_Domains.Dtos
{
    public class RefeicaoDto
    {
        public Guid Id { get; set; }
        public string NomeRef { get; set; }
        public DateOnly Data { get; set; }
        public List<AlimentoConsumidoDto> Alimentos { get; set; }
        public double TotalCalorias { get; set; }
        public double TotalProteinas { get; set; }
        public double TotalCarboidratos { get; set; }
        public double TotalGorduras { get; set; }
    }
}
