using System.ComponentModel.DataAnnotations;

namespace WebApiTraductorPMV.Dtos
{
    public class ScheduleEditMessageModel
    {
        /// <summary>
        /// Id que identifica el schedule message en la memoria del panel
        /// </summary>
        [Required]
        public string? Id { get; set; }

        /// <summary>
        /// Fecha en formato Epoch timestamp (numero de segundos desde 1970) en formato GTM
        /// </summary>
        [Required]
        public UInt32 Date { get; set; }


        /// <summary>
        /// Numero de fila donde se ubica el mensaje, entre 1 y 255, numeral 5.6.8.2 NTCIP 1203 v03.05
        /// </summary>
        [Required]
        [Range(1, 512)]
        public int MessageNumber { get; set; }

    }
}
