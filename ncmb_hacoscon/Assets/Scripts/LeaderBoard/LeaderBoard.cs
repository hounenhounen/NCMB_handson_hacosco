using NCMB;
using System.Collections;
using System.Collections.Generic;

public class LeaderBoard {
	
	public int currentRank = 0;
	public List<NCMB.Rankers> topRankers = null;
	
	// サーバーからトップ5を取得 ---------------    
	public void fetchTopRankers()
	{
		// データストアの「Time」クラスから検索
		NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject> ("Time");
		query.OrderByAscending ("time");
		query.Limit = 5;
		
		query.FindAsync ((List<NCMBObject> objList ,NCMBException e) => {
			
			if (e != null) {
				//検索失敗時の処理
			} else {
				//検索成功時の処理
				List<NCMB.Rankers> list = new List<NCMB.Rankers>();
				// 取得したレコードをtimeクラスとして保存
				foreach (NCMBObject obj in objList) {
					string    t = System.Convert.ToString(obj["time"]);
					list.Add( new Rankers(t) );
				}
				topRankers = list;
			}
		});
	}
	
}