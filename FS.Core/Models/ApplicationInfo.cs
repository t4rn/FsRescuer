using System;

namespace FS.Core.Models
{
    /// <summary>
    /// morp._fs_all
    /// </summary>
    public class ApplicationInfo
    {
        public int Id { get; set; }
        public string AppNumber { get; set; }
        public string AnalyseStatus { get; set; }
        public bool IsOnOldFs { get; set; }
        public bool IsOnNewFs { get; set; }
        public string AnalyseMessage { get; set; }
        public decimal? OldFsSize { get; set; }
        public decimal? NewFsSize { get; set; }
        public string MoveStatus { get; set; }
        public bool IsMoved { get; set; }
        public string MoveMessage { get; set; }
        public decimal? MovedMb { get; set; }
    }
}
