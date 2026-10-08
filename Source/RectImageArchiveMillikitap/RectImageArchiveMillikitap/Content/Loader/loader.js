var loader_number = 1;
var preloader_number = 1;


window.onload = ()=>{
    setTimeout(()=>{
        $(`.loader-${loader_number}`).addClass('complete');
        setTimeout(()=>{
            $(`.preloader-${preloader_number}`).addClass('complete');
            setTimeout(()=>{
                $(`.preloader-${preloader_number}`).css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000);
    const html_code = $('#html_code');
    const js_code = $('#js_code');
    html_code.text("<div class=\"preloader-1\"><div class=\"loader-1\"></div></div>");
    js_code.text(`var loader_number = ${loader_number}; var preloader_number = ${preloader_number};`);
}; 


function Loader1()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(loader_number != 1)
    {
        nr.addClass('loader-1');
        nr.removeClass(`loader-${loader_number}`);
        loader_number = 1;
        nr = $(`.loader-${loader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};

function Loader2()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(loader_number != 2)
    {
        nr.addClass('loader-2');
        nr.removeClass(`loader-${loader_number}`);
        loader_number = 2;
        nr = $(`.loader-${loader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};

function Loader3()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(loader_number != 3)
    {
        nr.addClass('loader-3');
        nr.removeClass(`loader-${loader_number}`);
        loader_number = 3;
        nr = $(`.loader-${loader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};

function Loader4()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(loader_number != 4)
    {
        nr.addClass('loader-4');
        nr.removeClass(`loader-${loader_number}`);
        loader_number = 4;
        nr = $(`.loader-${loader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 4000); 
};

function Loader5()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(loader_number != 5)
    {
        nr.addClass('loader-5');
        nr.removeClass(`loader-${loader_number}`);
        loader_number = 5;
        nr = $(`.loader-${loader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 4000); 
};

function Loader6()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(loader_number != 6)
    {
        nr.addClass('loader-6');
        nr.removeClass(`loader-${loader_number}`);
        loader_number = 6;
        nr = $(`.loader-${loader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};


/* PreLoaders */

function PreLoader1()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(preloader_number != 1)
    {
        prenr.addClass('preloader-1');
        prenr.removeClass(`preloader-${preloader_number}`);
        preloader_number = 1;
        prenr = $(`.preloader-${preloader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};

function PreLoader2()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(preloader_number != 2)
    {
        prenr.addClass('preloader-2');
        prenr.removeClass(`preloader-${preloader_number}`);
        preloader_number = 2;
        prenr = $(`.preloader-${preloader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};

function PreLoader3()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(preloader_number != 3)
    {
        prenr.addClass('preloader-3');
        prenr.removeClass(`preloader-${preloader_number}`);
        preloader_number = 3;
        prenr = $(`.preloader-${preloader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};

function PreLoader3()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(preloader_number != 3)
    {
        prenr.addClass('preloader-3');
        prenr.removeClass(`preloader-${preloader_number}`);
        preloader_number = 3;
        prenr = $(`.preloader-${preloader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};

function PreLoader4()
{
    var prenr = $(`.preloader-${preloader_number}`);
    var nr = $(`.loader-${loader_number}`);
    prenr.css('display', 'block');
    prenr.removeClass('complete');
    nr.removeClass('complete');

    if(preloader_number != 4)
    {
        prenr.addClass('preloader-4');
        prenr.removeClass(`preloader-${preloader_number}`);
        preloader_number = 4;
        prenr = $(`.preloader-${preloader_number}`);
    }
    refresh()
    setTimeout(()=>{
        nr.addClass('complete');
        setTimeout(()=>{
            prenr.addClass('complete');
            setTimeout(()=>{
                prenr.css('display', 'none');
            }, 1000);
        }, 300);
    }, 2000); 
};

function refresh()
{
    const html_code = $('#html_code');
    const js_code = $('#js_code');
    html_code.text(`<div class=\"preloader-${preloader_number}\"><div class=\"loader-${loader_number}\"></div></div>`);
    js_code.text(`var loader_number = ${loader_number}; var preloader_number = ${preloader_number};`);
}