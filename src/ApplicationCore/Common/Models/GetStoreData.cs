using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Common.Models
{
    public class GetStoreDataClass
    {
        public int? StoreId { get; set; }
        public int? StoreDataId { get; set; }
        public int? PlazaId { get; set; }
        public int? DistrictId { get; set; }
        public int? ZonaId { get; set; }
        public string Tienda { get; set; }
        public string Code { get; set; }
        public string StoreName { get; set; }
        public string Version { get; set; }
        public string StoreIp { get; set; }
        public string Zona { get; set; }
        [Column("CR Plaza")]
        public string CRPlaza { get; set; }
        public string Plaza { get; set; }
        public string District { get; set; }
        public DateTime? LastReg { get; set; }
        public string Segmento { get; set; }
        public string Formato { get; set; }
        public string StatusTienda { get; set; }
        public int? ConnectionType { get; set; }
        public string Status { get; set; }
        public string EstatusVenta { get; set; }
        public string Mac { get; set; }
        public string TimeZone { get; set; }
        public DateTime? LocalDateTime { get; set; }
        public DateTime? DateRegistration { get; set; }
        public DateTime? DateTransitionF2 { get; set; }
        public int? SEStatus { get; set; }
        public Guid? Uid { get; set; }
        public int? TillCount { get; set; }
        public bool? EstatusApertura { get; set; }
        public bool? SecureConnection { get; set; }
    }
}
