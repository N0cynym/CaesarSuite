﻿using Caesar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diogenes.DiagnosticProtocol
{
    public class KW2C3PE : BaseProtocol
    {
        private static bool EnterDiagnosticSession(ECUConnection connection)
        {
            Console.WriteLine("KW2C3PE: Switching session states");
            byte[] sessionSwitchResponse = connection.SendMessage(new byte[] { 0x10, 0x92 });
            byte[] sessionExpectedResponse = new byte[] { 0x50, 0x92 };
            if (!sessionSwitchResponse.Take(2).SequenceEqual(sessionExpectedResponse))
            {
                Console.WriteLine($"Failed to switch session : target responded with [{BitUtility.BytesToHex(sessionSwitchResponse, true)}]");
                return false;
            }
            return true;
        }

        private static bool GetVariantID_1A86(ECUConnection connection, out int variantId)
        {
            byte[] variantQueryResponse = connection.SendMessage(new byte[] { 0x1A, 0x86 });
            byte[] variantExpectedResponse = new byte[] { 0x5A, 0x86 };

            if (!variantQueryResponse.Take(2).SequenceEqual(variantExpectedResponse))
            {
                variantId = 0;
                return false;
            }
            else
            {
                variantId = (variantQueryResponse[12] << 8) | variantQueryResponse[13];
                return true;
            }
        }
        private static bool GetVariantID_1A87(ECUConnection connection, out int variantId)
        {
            byte[] variantQueryResponse = connection.SendMessage(new byte[] { 0x1A, 0x87 });
            byte[] variantExpectedResponse = new byte[] { 0x5A, 0x87 };

            if (!variantQueryResponse.Take(2).SequenceEqual(variantExpectedResponse))
            {
                variantId = 0;
                return false;
            }
            else
            {
                variantId = (variantQueryResponse[4] << 8) | variantQueryResponse[5];
                return true;
            }
        }

        private static bool GetVariantID(ECUConnection connection, out int variantId)
        {
            if (GetVariantID_1A86(connection, out int idFor1A86))
            {
                variantId = idFor1A86;
                return true;
            }
            if (GetVariantID_1A87(connection, out int idFor1A87))
            {
                variantId = idFor1A87;
                return true;
            }
            variantId = 0;
            return false;
        }

        public override void ConnectionEstablishedHandler(ECUConnection connection)
        {
            if (!EnterDiagnosticSession(connection))
            {
                return;
            }
            if (GetVariantID(connection, out int variantId))
            {
                connection.VariantIsAvailable = true;
                connection.ECUVariantID = variantId;
                Console.WriteLine($"Variant has been successfully configured as {(variantId & 0xFFFF):X4}");
            }
            else
            {
                Console.WriteLine("KW2C3PE: Could not identify variant (1A86, 1A87)");
                return;
            }
        }
        public override string GetProtocolName()
        {
            return "KW2C3PE";
        }

        public override bool SupportsUnlocking()
        {
            return true;
        }
    }
}
