using ECommons.Configuration;

namespace ICE.ConfigFiles;

public partial class Config
{
    public int ConfigVersion = 1;
    public int Config_Versioning { get; set; } = 2;
    public bool OldConfigMigrateV1 = false;

    public void Save()
    {
        EzConfig.Save();
    }

    public void SaveDebounced()
    {
        EzConfigExtensions.SaveDebounced();
    }
}
