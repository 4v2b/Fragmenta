import { useTranslation } from 'react-i18next';
import { changeLanguage } from '@/i18n';

const lngs = {
  en: { nativeName: 'Eng' },
  uk: { nativeName: 'Укр' }
};

export function LanguageSwitch() {
  const { i18n } = useTranslation();
  return (

    <div>
      {Object.keys(lngs).map((lng) => (
        <button key={lng} style={{ fontWeight: i18n.resolvedLanguage === lng ? 'bold' : 'normal', padding: '0.3em' }} type="submit" onClick={() => changeLanguage(lng)}>
          {lngs[lng].nativeName}
        </button>
      ))}
    </div>
  )
}