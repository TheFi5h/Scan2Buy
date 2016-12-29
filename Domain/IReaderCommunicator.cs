using System.Collections.Generic;

namespace Domain
{
    public interface IReaderCommunicator
    {
        
        // Connection
        bool Connect();

        bool Disconnect();

        // Activation
        void ActivateScan();

        void DeactivateScan();

        // Getting Result
        IList<TagData> GetScannedTags();
    }
}
