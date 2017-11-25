﻿using System;
using System.IO;
using System.Text;
using YaR.MailRuCloud.Api.Base.Requests.Web;

namespace YaR.MailRuCloud.Api.Base.Requests.Mobile
{
    public class MobMetaServerRequest : BaseRequestString<MobMetaServerRequest.Result>
    {
        public MobMetaServerRequest(CloudApi cloudApi) : base(cloudApi)
        {
        }

        protected override string RelationalUri => "https://dispatcher.cloud.mail.ru/m";

        protected override RequestResponse<Result> DeserializeMessage(string data)
        {
            var datas = data.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            var msg = new RequestResponse<Result>
            {
                Ok = true,
                Result = new Result
                {
                    Url = datas[0],
                    Ip = datas[1],
                    Unknown = int.Parse(datas[2])
                }
            };
            return msg;
        }

        public class Result
        {
            public string Url { get; set; }
            public string Ip { get; set; }
            public int Unknown { get; set; }
        }
    }
}