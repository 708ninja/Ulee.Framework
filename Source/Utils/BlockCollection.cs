using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace Ulee.Utils
{
    public enum EBlockListException
    {
        Default = 0,
        Empty,
        ArrayIndex
    }

    public class BlockListException : ApplicationException
    {
        public int Code { get; private set; }

        public BlockListException(string msg = "Occurred Connection Exception!", int code = 0)
            : base(msg)
        {
            Code = code;
        }
    }

    public class UlBlockList<T>
    {
        public UlBlockList(int length)
        {
            blockPosition = 0;
            blockLength = length;

            blocks = new List<T[]>();
            blocks.Add(new T[blockLength]);

            elementLength = Marshal.SizeOf<T>();
        }

        private int blockPosition;
        private int blockLength;
        private int elementLength;
        private List<T[]> blocks;

        public T this[int index]
        {
            get
            {
                int i = index / blockLength;
                int j = index % blockLength;

                if (i >= blocks.Count)
                {
                    throw new BlockListException("Invalid reference index exception!", (int)EBlockListException.ArrayIndex);
                }
                if ((i == (blocks.Count-1)) && (j >= blockPosition))
                {
                    throw new BlockListException("Invalid reference index exception!", (int)EBlockListException.ArrayIndex);
                }

                return blocks[i][j];
            }
        }

        public int Count
        { get { return (blocks.Count - 1) * blockLength + blockPosition; } }

        public void Lock()
        {
            Monitor.Enter(blocks);
        }

        public void Unlock()
        {
            Monitor.Exit(blocks);
        }

        public void Clear()
        {
            lock (blocks)
            {
                blocks.Clear();
                blocks.Add(new T[blockLength]);

                blockPosition = 0;
            }
        }

        public void Add(T value)
        {
            lock (blocks)
            {
                if (blockPosition >= blockLength)
                {
                    blocks.Add(new T[blockLength]);
                    blockPosition = 0;
                }

                blocks[blocks.Count - 1][blockPosition++] = value;
            }
        }

        public void AddRange(T[] value)
        {
            if (value == null) return;

            lock (blocks)
            {
                int restLength = blockLength - blockPosition;

                if (restLength <= 0)
                {
                    blocks.Add(new T[blockLength]);
                    blockPosition = 0;
                    restLength = blockLength;
                }

                if (value.Length <= restLength)
                {
                    Buffer.BlockCopy(value, 0, blocks[blocks.Count - 1], blockPosition * elementLength, value.Length * elementLength);
                    blockPosition += value.Length;
                }
                else
                {
                    int start = 0;
                    int srcLength = value.Length;

                    Buffer.BlockCopy(value, 0, blocks[blocks.Count - 1], blockPosition * elementLength, restLength * elementLength);
                    blockPosition = 0;
                    start += restLength;
                    srcLength -= restLength;

                    int count = srcLength / blockLength;
                    for (int i = 0; i < count; i++)
                    {
                        blocks.Add(new T[blockLength]);
                        blockPosition = 0;

                        Buffer.BlockCopy(value, start * elementLength, blocks[blocks.Count - 1], 0, blockLength * elementLength);
                        start += blockLength;
                        srcLength -= blockLength;
                    }

                    if (srcLength > 0)
                    {
                        blocks.Add(new T[blockLength]);
                        blockPosition = 0;

                        Buffer.BlockCopy(value, start * elementLength, blocks[blocks.Count - 1], 0, srcLength * elementLength);
                        blockPosition += srcLength;
                    }
                }
            }
        }

        public T[] ToArray()
        {
            if (Count == 0)
            {
                throw new BlockListException("BlockList is empty!", (int)EBlockListException.Empty);
            }

            int i;
            T[] array = new T[Count];

            lock (blocks)
            {
                for (i = 0; i < blocks.Count - 1; i++)
                {
                    Buffer.BlockCopy(blocks[i], 0, array, i * blockLength, elementLength * blockLength);
                }

                if (blockPosition > 0)
                {
                    Buffer.BlockCopy(blocks[i], 0, array, i * blockLength, elementLength * blockPosition);
                }
            }

            return array;
        }

        public T[] ToArray(int start, int step, int count)
        {
            if ((Count == 0) || (count == 0))
            {
                throw new BlockListException("BlockList is empty!", (int)EBlockListException.Empty);
            }

            T[] array = new T[count];

            lock (blocks)
            {
                for (int i = 0; i < count; i++)
                {
                    array[i] = this[start + i * step];
                }
            }

            return array;
        }

        public T[] ToArray(int start, double step, int count)
        {
            if ((Count == 0) || (count == 0))
            {
                throw new BlockListException("BlockList is empty!", (int)EBlockListException.Empty);
            }

            T[] array = new T[count];

            lock (blocks)
            {
                int pos = 0;

                for (int i = 0; i < count; i++)
                {
                    pos = start + (int)Math.Round(i * step);

                    if (pos < Count)
                        array[i] = this[pos];
                    else 
                        array[i] = array[i-1];
                }
            }

            return array;
        }
    }
}
