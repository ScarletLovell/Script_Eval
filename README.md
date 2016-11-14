# Script_Eval
___Eval at it's finest___

##Using this?
    There should be a folder inside the zip, extract the folder to your Add-Ons folder in Blockland.
###Having issues? Want something new?
Post them in the [Issues](https://github.com/Anthonyrules144/Script_Eval/issues) tab, please.
###Have a code request?
Post it in the [pull request](https://github.com/Anthonyrules144/Script_Eval/pulls) tab.

___

##Commands
* /grantEval [name]
* /takeEval [name]
* /eval [TYPE] [line]

___

##Things that are useful in my eval.
* Multilining. <br/>
 - As of now, multilining is used with `++`, and can be ended with `++end`
* Mutli-script support <br/>
 - Support for TorqueScript, BlocklandLua, Blockland-V8, and ZScript (My custom script)
* Customization <br/>
 - It's nice to have control over what you do in ***your*** game. this is how you want it to be.
* Granting / Taking from people, not every SA can use it<br/>
 - For being as advanced as can be, this tool is more then certain that it can crash your server if not used right, this is why I have granting.

___

##Changing the way you use eval in chat?
Check out the ___$Eval___ variable, you can change the name of the type, Use each ID for the ID it was meant for.<br/>
Current: `$eval = "TSLines++0 Torque@1 Torque$1 Torque+1 Torque\\1 Torque.1 Lua>>2 ZScript__3 JavaScript<<4";`<br/>
> EX: `$Eval = "Torque||1 ";` "Torque" is the word you're using, || is the way you're using it, and 1 is TorqueScripts ID
in chat use `||$abc = 123;`
