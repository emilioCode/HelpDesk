import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'filter'
})
export class FilterPipe implements PipeTransform {

  transform(array: any, args?: any): any {
    var list=[];
    if(args===undefined || args.length ==0) return array;
    
    //START FILTER EVERYWHERE
    for (let i = 0; i < array.length; i++) {
      //console.log(array[i])
      var c = 0;//conteo
      var s = false;
      Object.values(array[i]).forEach(p=>{
        if(p!=null){
          if(p.toString().toLowerCase().includes(args.toLowerCase())){
            c++;
          }
        }
        
      })

      if(c>0&&s==false){
        list.push(array[i]);
        c++;
        s=true;
      }
    }
    return list;
  }

}
