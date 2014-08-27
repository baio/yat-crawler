#library(SnowballC)
#library(Rgraphviz)
Sys.setlocale(,"russian")
#setwd("C:/dev/obl-crawler/obl-concepts-cloud")
corpus <- Corpus(DirSource("../data/alchemy-concepts", encoding = "UTF-8"))

replaceSpaces <- function(x){ gsub(" ", "_", x) }
replaceCommas <- function(x){ gsub(",", " ", x) }

corpus <- tm_map(corpus, replaceSpaces)
corpus <- tm_map(corpus, replaceCommas)

#corpus[[2]]
stemDocument(corpus[[2]])


corpus <- tm_map(corpus, stripWhitespace)
corpus <- tm_map(corpus, tolower)
corpus <- tm_map(corpus, removeWords, stopwords("english"))
#corpus <- tm_map(corpus, stemDocument)
#corpus <- tm_map(corpus, removePunctuation)
wordcloud(corpus, scale=c(5,0.5), max.words=50, random.order=FALSE, rot.per=0.35, use.r.layout=FALSE, colors=brewer.pal(8, "Dark2"))

                   