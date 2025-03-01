# Welcome to Translate-Console Repository

> [!WARNING]
> This console only supports Google Translate; APIs such as Deepl and Microsoft Azure AI Translate will be supported at a later date.

This is a tool that allows translations to be done on the console.

```bash
$ translate ja Hello World

> [Before]
>  Hello World
>  ↓
> [After: ja]
> ハローワールド
```

# ℹ️  How to Use

```bash
# ヘルプを表示
translate help

# 翻訳先の言語を指定して翻訳
translate en ハローワールド

# 結果をJSONで出力
translate json en ハローワールド
```

対応している言語の言語コード一覧は [言語サポート  |  Cloud Translation  |  Google Cloud](https://cloud.google.com/translate/docs/languages) を参照してください。

# ⚡ インストール

1. [Latest Release](https://github.com/SEZRi-Community/TranslateConsole/releases) からビルド済みバイナリをダウンロード

2. ダウンロードしたバイナリを解凍して好きなディレクトリへ移動します。

3. Google Translate (GAS) のAPIキーを発行します。
   
(3-1. Google アカウントにログインされている状態で [https://script.google.com/](https://script.google.com/) にアクセスします。
   
(3-2. 「新しいプロジェクト」からプロジェクトを作成します。
   
(3-3. 下記のコードを貼り付け、保存します。

```bash
const doGet = e => {
  if (e.parameter.text == undefined || e.parameter.to == undefined) {
    return ContentService.createTextOutput(JSON.stringify({code: 400, msg: "pram"}));
  }
  const text = decodeURI(e.parameter.text);
  const to = e.parameter.to;
  let from = "";
  let translated = undefined;
  
  if (e.parameter.from != undefined) from = e.parameter.from;

  try {
    translated = LanguageApp.translate(text, from, to);
  } catch (e) {
    console.error("Translation error: " + e.message);
    return ContentService.createTextOutput(JSON.stringify({code: 400, msg: "unexpected", details: e.message}));
  }

  return ContentService.createTextOutput(JSON.stringify({code: 200, msg: "success", text: translated}));
}
```

(3-4. 右上にある「デプロイ」を押し、「新しいデプロイ」を選択します。

(3-5. 左上の歯車⚙から「ウェブアプリ」を選択します。

(3-6. アクセスできるユーザーを「全員」とし、「デプロイ」を選択します。

(3-7. 表示されているURLをコピーします。

### これでAPIキーの作成ができました。

5. `config.json`を同じ階層に作成し`3.`で発行したAPIキー(URL)を設定します。

```json
{"api": "<ここにAPIキーを設定>"}
```

### これで完了です🎉

#  トラブルシューティング
・一瞬ウィンドウが表示されるだけで起動しない   
   config.json が実行ファイルと同じ階層に含まれていることを確認してください。同じ階層に含まれているにも関わらずこのような挙動に至る場合は、config.json の構文が正しいことをご確認ください。
