#!/usr/bin/python
# -*- coding: utf-8 -*-

__author__ = 'max'

import codecs
import goslate
from os import walk
import os.path

gs = goslate.Goslate()

i = 0

files = []
for (dirpath, dirnames, filenames) in walk("../data/contents"):
    files.extend(filenames)


for file in files:
    fpath = "../data/contents-en/" + file
    if not os.path.isfile(fpath):
        with codecs.open(fpath, "w", "utf-8") as w:
            with codecs.open("../data/contents/" + file, "r", "utf-8") as f:
                content = f.read()
                #print(content)
                trans = gs.translate(content, 'en', 'ru')
                #print(trans)
                w.write(trans)