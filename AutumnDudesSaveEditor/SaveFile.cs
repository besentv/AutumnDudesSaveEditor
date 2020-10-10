using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AutumnDudesSaveEditor
{
    public class SaveFile
    {
        public string FilePath { get; set; }
        public string Name { get; set; }
        public SaveType Type { get; set; }
        public ObservableCollection<ActionElementMap> ButtonMaps { get; set; }
        public ObservableCollection<ActionElementMap> AxisMaps { get; set; }
    }
}
