nrow(Dataset2)


load(file = "C:/Users/Anil/Desktop/Aarons-Email-attachments/crx.RData", envir = .GlobalEnv)

BSkyReloadDataset(fullpathfilename='C:/Users/Anil/Desktop/Aarons-Email-attachments/crx.RData',  filetype='RDATA', sheetname='', csvHeader=FALSE, loaddataonly=FALSE,isBasketData=FALSE, sepChar='', deciChar='', datasetname='Dataset2')


library(gapminder)
BSkyLoadRefreshDataframe(dframe=gapminder,load.dataframe=TRUE)
