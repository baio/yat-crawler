#!/usr/bin/python

__author__ = 'max'

import codecs
import goslate
from os import walk

gs = goslate.Goslate()

i = 0

files = []
for (dirpath, dirnames, filenames) in walk("../data/contents"):
    files.extend(filenames)


for file in files:
    with codecs.open("../data/contents-en/" + file, "w", "utf-8") as w:
        with open("../data/contents/" + file) as f:
            content = f.readtext()
            print(content)
            """
                content = f.readtext()
                for line in content:
                    #print line
                    trans = gs.translate(line, 'en', 'ru')
                    #print trans
                    w.write(trans + "\n")
            """