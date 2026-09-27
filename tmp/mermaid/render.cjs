const fs = require('fs');
const path = require('path');
const { chromium } = require('C:/Users/pickwme/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const sourcePath = process.argv[2] || 'docs/components.mmd';
const basename = process.argv[3] || 'components-mermaid';
(async () => {
  const browser = await chromium.launch({executablePath:'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless:true});
  const page = await browser.newPage({viewport:{width:1600,height:1200},deviceScaleFactor:2});
  await page.setContent('<html><head><style>html,body{margin:0;background:white}#diagram{display:inline-block;padding:32px}svg{max-width:none!important}</style></head><body><div id="diagram"></div></body></html>');
  await page.addScriptTag({path:path.resolve('tmp/mermaid/mermaid.min.js')});
  const source = fs.readFileSync(sourcePath,'utf8');
  const dims = await page.evaluate(async source => {
    mermaid.initialize({startOnLoad:false,theme:'default',securityLevel:'strict',fontFamily:'Arial',flowchart:{useMaxWidth:false,htmlLabels:false,curve:'basis',nodeSpacing:55,rankSpacing:100},themeVariables:{fontSize:'22px'}});
    const {svg} = await mermaid.render('components',source);
    document.getElementById('diagram').innerHTML=svg;
    const el = document.querySelector('svg');
    const v = el.viewBox.baseVal;
    el.setAttribute('width',v.width); el.setAttribute('height',v.height);
    return {width:Math.ceil(v.width+64),height:Math.ceil(v.height+64),svg:el.outerHTML};
  },source);
  fs.mkdirSync('output/pdf',{recursive:true});
  fs.writeFileSync(`output/pdf/${basename}.svg`,dims.svg);
  await page.setViewportSize({width:dims.width,height:dims.height});
  await page.pdf({path:`output/pdf/${basename}.pdf`,width:dims.width+'px',height:dims.height+'px',printBackground:true,margin:{top:0,right:0,bottom:0,left:0}});
  await page.locator('#diagram').screenshot({path:`output/pdf/${basename}.png`});
  console.log(JSON.stringify({width:dims.width,height:dims.height,pdf:`output/pdf/${basename}.pdf`}));
  await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});
