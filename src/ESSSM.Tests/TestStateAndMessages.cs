using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ESSSM.Tests
{
    public enum States
    {
        NA,
        One,
        Two,
        Three,
        Four,
        Five
    };

    public class Ctx
    {
        public string Id;
        public int Count;
        public States CurrentState;
    }

    public class MessageA
    {

    }

    public class MessageB
    {
        public string MessageId { get; set; }
    }

    public class MessageC
    {
        public string Id { get; set; }
    }
}
