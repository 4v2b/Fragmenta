import './App.css'
import { useTranslation } from 'react-i18next'

function App() {
  const { t } = useTranslation()
  return (
    <>{t("greeting")}</>
  )
}

export default App
