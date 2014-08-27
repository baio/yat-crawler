#Sys.setlocale("LC_CTYPE", "russian")
Sys.setlocale(,"russian")
#txt <- read.csv("../data/tags.txt", encoding = "UTF-8", header=T, sep='\t');
corpus <- Corpus(DirSource("../data/", encoding = "UTF-8"))
corpus <- tm_map(corpus, stripWhitespace)
corpus <- tm_map(corpus, tolower)
corpus <- tm_map(corpus, removePunctuation)
wordcloud(corpus, scale=c(5,0.5), max.words=200, random.order=FALSE, rot.per=0.35, use.r.layout=FALSE, colors=brewer.pal(8, "Dark2"))

