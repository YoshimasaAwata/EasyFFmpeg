using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyFFmpeg
{
    /// <summary>
    /// コーデックとライブラリの対応
    /// </summary>
    internal class CodecLibPair
    {
        /// <value>コーデック</value>
        public string Codec { get; set; }
        /// <value>コーデックのエンコード/デコードを行うライブラリのリスト</value>
        public List<string> ExcoderLibraryList { get; set; }

        /// <param name="codecName">コーデックの名前</param>
        /// <param name="encoders">エンコーダーのリスト</param>
        public CodecLibPair(string codecName, string[] encoders) 
        { 
            Codec = codecName;
            ExcoderLibraryList = new List<string>(encoders);
        }
    }
}
