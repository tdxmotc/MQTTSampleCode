> [!NOTE]
> 此地方為TDX MQTT介接的範例程式，若要尋找TDX API介接範例程式，請至 https://github.com/tdxmotc/SampleCode

# TDX運輸資料流通服務MQTT介接範例程式碼說明

為使開發者能快速在M2M環境下透過MQTT介接TDX運輸資料流通服務平臺之交通領域資料服務，在此提供數種程式語言實作連接TDX MQTT服務的範例程式碼提供開發者做參考。

> [!NOTE]
> TDX平臺自2024年12月X日起對外開放使用MQTT方式介接資料，開放初期仍有資料即時性、服務穩定性、使用便利性等層面的建議還有賴TDX會員回饋給平臺，預計到2025年X月底之前透過MQTT取得的資料皆不納入點數計算，請會員在改接之前審慎評估資料應用整合方式與改為MQTT介接資料可能產生的風險。

## MQTT與一般API介接資料相異之處
### API
Restful API採一問一答的方式，由Client端(會員)主動發起API呼叫、Server端(TDX)回傳資料。
**優點**
可由Client端控制呼叫頻率，或是需要資料時才呼叫AP。
**缺點**
若呼叫API頻率大於資料實際更新頻率，則會產生許多無效的呼叫行為(收到的資料都是一樣的)，造成Client端與Server端網路與運算資源的浪費。

### MQTT
由Client連線MQTT服務，訂閱資料項的Topic，當Server端有資料異動時主動推送資料給Client端。
**優點**
由Server端主動推送異動資料給Client端，減少許多不必要的呼叫行為。

## MQTT連線與認證授權

### 連線
為確保資料傳輸的安全性，平臺採用MQTTS進行連線與傳送資料。

| 參數 | 值 |
| ------ | ------ |
| Host | mqtt.transportdata.tw |
| Port | 8883 |
| Use TLS | True |

> [!TIP]
> 連線完成後，**可重複使用此連線進行訂閱與接收資料**，不需要每次訂閱或收到資料後就重新連線(頻繁斷連線將造成Client端與Server端不必要的資源浪費)。

### 認證與授權
MQTT連線時使用一組帳號、密碼與ClientId進行身分認證與唯一識別。
連線參數說明如下:
| 參數 | 描述 |
| ------ | ------ |
| ClientId | 連線MQTT使用的ClientId**識別碼**，可從TDX會員中心取得 |
| Username| 連線MQTT使用的**帳號**，可從TDX會員中心取得 |
| Password | 連線MQTT使用的**密碼**，可從TDX會員中心取得 |

> [!TIP]
> 1. TDX會員僅能訂閱(Subscribe)資料，無法推送(Publish)資料。
> 2. **同一組帳號、密碼與ClientId僅能建立一次連線，若嘗試建立第二條連線，則第一條連線會自動被中斷。**

## MQTT訂閱資料

### 訂閱Topic
在成功連線MQTT服務之後，會員可依對資料項的需求訂閱一或多個資料項對應的Topic，即可在資料有更新時收到資料。收到的資料為**JSON**格式(少部分GTFS資料為二進位資料)，且資料內容**完全符合運具資料標準**(會與API回傳的內容完全一致)。
下表為目前TDX平臺已提供的部分資料項Topic(完整資料服務項目請參閱TDX官網)，未來會陸續開放更多的資料項服務供會員使用:  

| 資料項 | 服務版本 | MQTT Topic |  
| ------ | ------ | ------ |
| 縣市公車營運通阻資料 | v2 | v2/Bus/Alert/City/{縣市代碼} | 
| 縣市公車營運通阻資料 | v3 | v3/Bus/Alert/City/{縣市代碼} |
| 公總公車營運通阻資料 | v2 | v2/Bus/Alert/InterCity |  
| 臺鐵動態營運通阻資料 | v3 | v3/Rail/TRA/Alert |   
| 高鐵即時營運通阻資料 | v2 | v2/Rail/THSR/AlertInfo | 
| 捷運(輕軌)營運通阻資料 | v2 | v2/Rail/Metro/Alert/{軌道系統代碼} |
| 航運營運通阻資料 | v3 | v3/Ship/Alert/International | 
| 縣市公車動態定時資料(A1) | v2 | v2/Bus/RealTimeByFrequency/City/{縣市代碼}/{路線名稱} | 
| 縣市公車動態定時資料(A1) | v3 | v3/Bus/RealTimeByFrequency/City/{縣市代碼}/{路線名稱} | 
| 公總公車動態定時資料(A1) | v2 | v2/Bus/RealTimeByFrequency/InterCity/{路線名稱} | 
| 縣市公車動態定點資料(A2) | v2 | v2/Bus/RealTimeNearStop/City/{縣市代碼}/{路線名稱} | 
| 縣市公車動態定點資料(A2) | v3 | v3/Bus/RealTimeNearStop/City/{縣市代碼}/{路線名稱} | 
| 公總公車動態定點資料(A2) | v2 | v2/Bus/RealTimeNearStop/InterCity/{路線名稱} | 

Client端訂閱Topic後，當**資料來源端的資料有異動時**，TDX平臺會即時將異動資料推送給Client端，收到的資料皆為該資料項的**最小單位**，以下用三個資料項目進行範例說明。

- ### Topic訂閱範例1: 捷運(輕軌)營運通阻資料

| 資料項 | MQTT Topic |  
| ------ | ------ |
| 臺北捷運營運通阻資料 | v2/Rail/Metro/Alert/TRTC |
| 全臺捷運(輕軌)營運通阻資料 | v2/Rail/Metro/Alert/# |

不論是訂閱上述兩者中哪一個Topic，皆會以**軌道系統**為單位收到資料，舉例說明:

#### 訂閱v2/Rail/Metro/Alert/TRTC，僅會收到TRTC的資料:  
```json
{
  "AuthorityCode": "TRTC",
  //省略其他欄位
  "Alerts": [
    {
    },
    //會有多筆TRTC的營運通阻訊息
  ]  
}
```

#### 訂閱v2/Rail/Metro/Alert/#，會收到所有捷運(輕軌)的資料:  
收到第一筆資料:
```json
{
  "AuthorityCode": "TRTC",
  //省略其他欄位
  "Alerts": [
    {
    },
    //會有多筆TRTC的營運通阻訊息
  ]  
}
```
收到第二筆資料:
```json
{
  "AuthorityCode": "KLRT",
  ...省略其他欄位...
  "Alerts": [
    { 
    },
    //會有多筆KLRT的營運通阻訊息
  ]  
}
```
- ### Topic訂閱範例2: 縣市公車動態定時資料(A1)

| 資料項 | MQTT Topic |  
| ------ | ------ |
| 臺北市公車動態定時資料(指定652路線) | v2/Bus/RealTimeByFrequency/City/Taipei/652|
| 臺北市公車動態定時資料(所有路線) | v2/Bus/RealTimeByFrequency/City/Taipei/# |
| 全臺公車動態定時資料(所有路線) | v2/Bus/RealTimeByFrequency/City/# |

不論是訂閱上述三者中哪一個Topic，皆會以**路線名稱**為單位收到資料，舉例說明:

#### 訂閱v2/Bus/RealTimeByFrequency/City/Taipei/652，僅會收到臺北市652路線的資料:
```json
[
  {
    //臺北市652公車路線的資料-第一台車
  },
  {
    //臺北市652公車路線的資料-第二台車
  },
  //會有多筆臺北市652公車資料
]
```

#### 訂閱v2/Bus/RealTimeByFrequency/City/Taipei/#，會收到全臺北市所有路線的資料:
收到第一筆資料(臺北市652路線所有車輛的資料):
```json
[
  {
    //臺北市652公車路線的資料-第一台車
  },
  {
    //臺北市652公車路線的資料-第二台車
  },
  //會有多筆臺北市652公車資料
]
```
收到第二筆資料(臺北市207路線所有車輛的資料):
```json
[
  {
    //臺北市207公車路線的資料-第一台車
  },
  {
    //臺北市207公車路線的資料-第二台車
  },
  //會有多筆臺北市207公車資料
]
```
#### 訂閱v2/Bus/RealTimeByFrequency/City/#，會收到全臺市區公車所有路線的資料:
收到第一筆資料(臺北市652路線所有車輛的資料):
```json
[
  {
    //臺北市652公車路線的資料-第一台車
  },
  {
    //臺北市652公車路線的資料-第二台車
  },
  //會有多筆臺北市652公車資料
]
```
收到第二筆資料(臺中市63路線所有車輛的資料):
```json
[
  {
    //臺中市63公車路線的資料-第一台車
  },
  {
    //臺中市63公車路線的資料-第二台車
  },
  //會有多筆臺中市63公車資料
]
```

### 指定QoS資料傳輸品質

因網路環境的不可控，MQTT通訊協定支援QoS(Quality of Service)設定；QoS定義了資料傳輸品質，可由Client端依自身的應用情境來決定是否需準確無誤的收到資料，或即使偶爾丟失資料也沒關係。

| QoS Level | 效果 |  網路頻寬與CPU資源使用量  |
| ------ | ------ | ------ |
| 0 | 資料不保證送達，可能會收不到少部分資料 | 最低 |
| 1 | 資料保證送達，但可能會重複收到資料 | 低 |
| 2 | 資料保證送達，且保證只會收到一筆資料 | 稍高 |

TDX在導入MQTT資料推播機制初期暫不收費，但未來開始將MQTT資料接收量納入點數計算之後，QoS的設定將些微影響到最終計算出來的點數量。以下列出不同的QoS等級對應適合的服務應用場景。

| QoS Level | 適合的應用服務 |
| ------ | ------ |
| 0 | 服務允許偶發性的資料遺漏 |
| 1 | 服務不允許資料遺漏，但可接受重複收到資料 |
| 2 | 服務不允許資料遺漏，且不接受重複收到資料 |

> [!TIP]
> #### TDX是怎麼判定會員有收到資料?
> 在傳統API，TDX是依照會員發起的HTTP Request API次數和回傳資料量來計算點數
。在MQTT機制下，TDX是使用MQTT的ack機制來決定會員是否有收到資料，若TDX MQTT Server在傳送資料給會員Client端後，Client端有回覆ack，則表示Client有收到資料 (**ack機制通常由各個程式語言的MQTT套件底層實作，開發者不需額外傳送ack訊息給Server端**)。唯一例外的是QoS 0因屬於送後不理的方式 (Server送出資料後，不會管Client是否有收到)，因此若Client端是使用QoS 0則不會有ack訊息回送給Server，在此限制之下**TDX在送出資料後，不論Client端是否有收到資料，一律視為Client已收到資料，因此資料使用次數+1(納入點數計算)**。

| 取用資料方式 | TDX資料呼叫(使用)次數+1時機 |
| ------ | ------ |
| API | TDX回應HTTP Status 200正常回傳資料 |
| MQTT(QoS 0) | TDX送出資料(不論會員是否有收到資料) |
| MQTT(QoS 1) | TDX收到ack訊息(保證會員收到資料才+1，若重複傳送則依重傳次數累加) |
| MQTT(QoS 2) | TDX收到ack訊息(保證會員收到資料才+1，且不會重複計算) |

> [!TIP]
> MQTT推播資料機制開放初期暫不收費，建議會員利用這段期間進行長時間測試，依照資料應用情境與上述MQTT特性來決定要使用何種QoS。 

### CleanSession參數
CleanSession為MQTT內的重要參數，代表Client端在斷線復連後是否要補收到斷線期間內未收到的資料。TDX透過MQTT推送的皆屬動態資料性質，Client端無需收到一堆斷線期間未收到的舊資料，因此CleanSession參數設定為true即可(通常預設值也為true)。

### 斷線重連
為了防止未預期的服務異常或網路環境不穩定所造成的MQTT連線中斷，Client端需自行實作MQTT連線中斷後自動復連機制；各個程式語言的MQTT套件通常都會提供連線中斷的Event Callback，開發者只需在事件裡重新呼叫MQTT連線方法即可，實作方式可參考範例程式碼。





