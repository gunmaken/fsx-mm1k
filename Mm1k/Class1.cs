using System;
using System.Collections.Generic;
using System.IO;

//muを一定にする
//K=50
//FinishTIme=10000理想(100000)
//廃棄率(対数)
namespace Hoge
{
    record Element
    {
        public double StartStayTime { get; set; } = default; //パケットが待機し始めた時刻
        public double StaySystemTime { get; set; } = default; //パケットのシステム滞在時間
        public int Num { get; set; } = default; //パケットのナンバリング
    }

    class Event
    {
        public double Time { get; set; } = default; //イベント時刻
        public int Type { get; set; } = default; //0がイベント発生, 1がサービス終了
        public Element Element { get; set; } = new Element(); //イベントのパケット
    }

    class Server
    {
        public bool IsUsing { get; set; } = false; //サービスの稼働状況
        public Element Element { get; set; } = new Element(); //サーバを使用するパケット
    }

    public class MM1K
    {
        List<Event> Table; //イベントテーブル
        Queue<Element> WaitQueue; //待ち行列
        Server Server; //サーバを表すクラス
        public double Lambda { get; private set; } //平均到着率
        public double Mu { get; private set; } //平均サービス率
        public int K { get; private set; } //システム容量
        public double FinishTime { get; private set; } //シミュレーション終了時刻
        public double StartTime { get; private set; } //シミュレーション開始時刻
        private int _tableCount; //イベントテーブルのインデックス
        List<Element> Elements; //パケットのリスト
        double _rho;
        public double AveragePacket { get; private set; }
        public double AverageTime { get; private set; }
        public double AverageDisposePercentage { get; private set; }
        public double N { get; private set; }
        public double W { get; private set; }
        public double Pk { get; private set; }

        //コンストラクタでフィールドに代入
        public MM1K(double lamda, double mu, int k, double finishTime, double startTime)
        {
            Table = new List<Event>();
            WaitQueue = new Queue<Element>();
            Server = new Server();
            this.Lambda = lamda;
            this.Mu = mu;
            this.K = k;
            this.FinishTime = finishTime;
            this.StartTime = startTime;
            this._tableCount = 0;
            this.Elements = new List<Element>();
            _rho = Math.Round((lamda / mu), 2);
        }
        
        //コンストラクタのオーバーロード
        public MM1K() : this(0.5, 1.0, 100, 10000.0, 0.0)
        {
        }

        //次のイベントの時間生成
        double GenerateEvent(double percentage)
        {
            var r = new Random();
            return (-Math.Log(1.0 - r.NextDouble()) / percentage);
        }
        
        //キューに追加
        void AddQueue(Element e)
        {
            var time = Table[_tableCount].Time;
            WaitQueue.Enqueue(e);
        }
        
        //サービス開始
        void StartService(Element e)
        {
            var time = Table[_tableCount].Time;

            var addTime = GenerateEvent(Mu);//サービス時間決定
            Table.Add(new Event{ //サービス終了のイベント追加
                Time = time + addTime,
                Type = 1,
                Element = e
            });

            e.StaySystemTime = addTime + time - e.StartStayTime;

            //サーバに使用状況を追加
            Server.Element = e;
            Server.IsUsing = true;    
        }

        void EndService(Element e)
        {
            var time = Table[_tableCount].Time;
            Server.IsUsing = false;
            if (WaitQueue.Count() != 0)
            {
                StartService(WaitQueue.Dequeue());
            }
            Elements.Add(e);
        }

        void Dispose(Element e)
        {
            e.StaySystemTime = 0.0;
            Elements.Add(e);
        }

        public void Report()
        {
            var time = FinishTime - StartTime;
            AveragePacket = Elements.Sum(x => x.StaySystemTime) / time;
            AverageTime = Elements.Sum(x => x.StaySystemTime) / Elements.Count();
            AverageDisposePercentage = (double)Elements.Count(x => x.StaySystemTime == 0.0) / Elements.Count();

            /*
            Console.WriteLine();
            Console.WriteLine($"{this.Lambda} {this.Mu}");
            Console.WriteLine($"平均パケット数: {AveragePacket}");
            Console.WriteLine($"平均システム滞在時間: {AverageTime}");
            Console.WriteLine($"パケット廃棄率: {AverageDisposePercentage}");
            */
        }

        public void Theoretical()
        {
            if (_rho != 1.0)
            {
                Pk = ((1.0 - _rho) * Math.Pow(_rho, K)) / (1 - Math.Pow(_rho, K));
                N = (_rho * (1.0 - Math.Pow(_rho, K + 1)) - (double)(K + 1)*(Math.Pow(_rho, K + 1)) * (1 - _rho)) / ((1.0 - _rho) * (1.0 - Math.Pow(_rho, K + 1)));
            }
            else
            {
                Pk = 1.0 / (K + 1.0);
                N = (double)K / 2;
            }
            
            W = N / (this.Lambda * (1 - Pk));
        }

        void Finally()
        {
            if(WaitQueue.Count() != 0)
            {
                foreach(var i in WaitQueue)
                {
                    i.StaySystemTime = FinishTime - i.StartStayTime;
                    Elements.Add(i);
                }
            }
        }
        public void StartSimulation()
        {
            bool IsRunning = false;

            //最初の発生時刻決定
            var addTime = GenerateEvent(Lambda); 
            Table.Add(new Event{
                Time = StartTime + addTime,
                Type = 0,
                Element = new Element()
            });

            //イベントテーブルの要素が0でなければフラグをtrueにする
            if (Table.Count() != 0) IsRunning = true;
            while (IsRunning)
            {
                //イベント発生の場合
                if (Table[_tableCount].Type == 0)
                {
                    Table[_tableCount].Element.StartStayTime = Table[_tableCount].Time;

                    var e = new Element(); //追加するパケットの変数
                    e.Num = Table[_tableCount].Element.Num + 1;
                    addTime = GenerateEvent(Lambda); //次のイベント生成
                    Table.Add(new Event{ //イベント表に追加
                        Time = Table[_tableCount].Time + addTime,
                        Type = 0,
                        Element = e
                    });

                    //サービス継続中か
                    if (Server.IsUsing) 
                    {
                        //待ち行列の空き容量がK-2か
                        if (WaitQueue.Count() <= K - 2)
                        {
                            AddQueue(Table[_tableCount].Element);
                        }
                        else
                        {
                            Dispose(Table[_tableCount].Element);
                        }
                    }
                    else
                    {
                        StartService(Table[_tableCount].Element);
                    }
                }
                
                //サービス終了の場合
                else
                {
                    EndService(Table[_tableCount].Element);
                }

                Table = Table.OrderBy(x => x.Time).ToList(); //イベントテーブルを時刻ごとにソート
                _tableCount++;

                if (Table[_tableCount].Time > FinishTime)
                {
                    IsRunning = false;
                }
            }
            Finally();
            Report();
            Theoretical();
        }
    }
}