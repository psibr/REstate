using System;

namespace REstate.Engine
{
    public class StringSerializer
    {
        private readonly Func<object, string> _Serializer;
        private readonly Func<string, object> _Deserializer;

        public StringSerializer(Func<object, string> serializer, Func<string, object> deserializer)
        {
            _Serializer = serializer;
            _Deserializer = deserializer;
        }

        protected StringSerializer()
        {
        }

        public virtual string Serialize(object o)
        {
            return _Serializer(o);
        }

        public virtual T Deserialize<T>(string s)
        {
            return (T)_Deserializer(s);
        }
    }
}
