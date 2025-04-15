# Snapmaw
This is a simple companion program for [croc](https://github.com/schollz/croc) on Windows. It adds a context menu option to files and folders that generates a link that can be sent to others with Snapmaw installed. Clicking the link automatically opens Snapmaw, which then uses croc to download the file/folder.

## Register / Unregister
To add the context menu option and the `snapmaw:` URL scheme, which is used by the generated links, you need to run this command as admin:
```sh
snapmaw --register
```
This adds the necessary entries to the registry. If you want to remove them again, you can simply run this command as admin:
```sh
snapmaw --unregister
```

## Usage
Once registered, simply right-click a file or folder in the file explorer and choose "Copy share link". This will automatically send the file using croc, and it will also copy a link to the clipboard. This link can then be shared with someone else that has Snapmaw registered.

To receive a file, simply open the link. Your browser may ask if you are okay with opening a "snapmaw:" link, which you need to accept to download the file. This will open a folder dialog that lets you select a folder to put the file/folder in, and then it will run croc to download it.

If the recipient doesn't have Snapmaw, they can still use croc to download the file. The link is not just a link, it is also the croc code-phrase for the file/folder, so just run `croc <link>` and enter `n` for the prompt asking if you wanted to send something instead.

## Naming
The name and generated links are based on machines in the Horizon games. Snapmaws are based on crocodiles, so the name is also a reference to croc.
