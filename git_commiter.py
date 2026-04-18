import os

commit = str(input('input commit text '))
rep_link = str(input('input repository link '))

commands = ["git init", "git add *.*", f'git commit -m "{commit}"', "git remote remove origin", f"git remote add origin {rep_link}", "git config –-system http.sslverify false", "git push -u origin master"]

for cmd in commands:
    exit_status = os.system(cmd)
    print(f"Exit Status: {exit_status}")
