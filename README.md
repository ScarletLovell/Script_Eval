# Script_Eval
___Eval at it's finest___

## Commands
* /eval [discriminator] [line]
* /grantEval [name or BL_ID]
* /takeEval [name or BL_ID]
* /silentEval
___

## Things that are probably useful in my script_eval add-on.
* Mutli-language support
  * Support for all custom languages in BL just by changing a pref.
* Customization
  * This add-on I made because I got annoyed at how others have barely any customization. And so, it has been in the works for 2+ years as one of the more known eval add-ons.
* Granting / Taking from people, not every SA can use it
  * For being as advanced as can be, this tool is more then certain that it can crash your server if not used right, this is why I have granting.
* There is preferences now!
  * You can use prefs via Blockland glass or RTB.
* Silent Eval
  * Silent eval lets you silently use eval whilst in chat
* ZScript
  * v2 of Script_Eval includes a very pre-alpha version of Zap Script
  * ZScript will also eventually be moved to DLL form as a custom language so be aware of this.

___

* Multilining.
  * Multilining lets you evaluate a bunch of lines of code from just in chat

By default, the discrim for Multilining is __++__, here's an example (with results): <br/>
* Added talk("hello world!"); to TSLines
 > ++talk("Hello World!"); <br/>
* 1: talk("Hello world!");
 > ++view
* (will talk in chat "Hello world!")
 > ++end

You can also use cancel to __cancel__ all the TSLines you've inputted <br/>
If needed, you can also use __edit # [words]__ to change a line #, see __view__ above.

___

## Changing the way you use eval in chat?
Check out the $Pref::Server::ScriptEval::Usages variable: <br/>
you can change the name of the type, Use each ID for the ID it was meant for. <br/>
> EX: `$Pref::Server::ScriptEval::Usages = "Torque:eval|@";`<br/>
> Torque is the name, eval is the function, and @ is the discriminator <br/>

> You can also change it by the preferences.

___

### Having issues? Want something new?
Post them in the [Issues](https://github.com/Anthonyrules144/Script_Eval/issues) tab, please.
### Have a code request?
Post it in the [pull request](https://github.com/Anthonyrules144/Script_Eval/pulls) tab.
