﻿namespace BestSockets
{
    public interface IObjectSerializer
    {
        byte[] Serialize(object obj);
        object Deserialize(byte[] bytes);
    }
}
