# waifu2x-chainer_multilang_gui
![-1sss](https://user-images.githubusercontent.com/16046279/37682587-5acead9a-2ccd-11e8-9951-bedda3a41d01.png)
Multilingual GUI for waifu2x-chainer (https://github.com/tsurumeso/waifu2x-chainer). Localization can be done via user-editable xaml file.

This project was forked from https://github.com/MaverickTse/waifu2x_caffe_multilang_gui

## Installation
1. You must install [waifu2x-chainer](https://github.com/tsurumeso/waifu2x-chainer) beforehand.
2. Set the "waifu2x-chainer path" on the Preferences tab appropriately.

## Localization
1. The localization files have the name UILang._language-code_.xaml; where _language-code_ is a 5-character identifier like en-US, zh-TW, ja-JP.
2. Make a copy of one of the bundled localization files.
3. Rename the file with your target language code replacing the original.
4. Using a text editor that support UTF-8, make the following changes:
  * ```<sys:String x:Key="ResourceDictionaryName">waifu2xui-en-US</sys:String>```
  * Replace en-US with the target language code
  * All the text enclosed by the ```<sys:String>``` tags
5. About language code:
  * Make up from _ab_-_XY_
  * ab can be found [Here](http://www.loc.gov/standards/iso639-2/php/langcodes-search.php) as Alpha-2 codes
  * XY can be found [Here](https://www.iso.org/obp/ui/#search)
  * Essentially _ab_ is the language, _XY_ is the country
  
## License and Sharing
* Do what the fuck you want with this soft
* No warranty attached
